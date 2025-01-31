using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class AdditionalParticle
{
    public Transform positionReference; // Reference point for spawning
    public GameObject particlePrefab;
    public Vector3 spawnOffset;
    public float duration = 1f;
    public bool spawnAsChild;
    public bool useObjectRotation = true;
}

[AddComponentMenu("Weapons/Weapon")]
public class Weapon : MonoBehaviour
{
    private GunRecoil recoilScript;

    [Header("References")]
    [Space(10)]
    [Tooltip("Reference to the weapon sway component")]
    [SerializeField] private WeaponSway weaponSway;
    [Tooltip("Reference to the gun bobbing component")]
    [SerializeField] private GunBobbing gunBobbing;

    [Header("Magazine Settings")]
    [Space(10)]
    [Tooltip("Maximum number of bullets in magazine")]
    [SerializeField] private int magazineSize = 30;
    [Tooltip("Current magazine size (View-only)")]
    [SerializeField] private int currentMagazineSize; // View-only in inspector
    private int currentBullets;

    [Header("UI Settings")]
    [Space(10)]
    [SerializeField] private TextMeshProUGUI magazineText; // Total magazine size display
    [SerializeField] private TextMeshProUGUI currentAmmoText; // Current ammo display
    
    [Header("Progress Bar Settings")]
    [Space(5)]
    [SerializeField] private Image reloadProgressBar;
    [SerializeField] private CanvasGroup progressBarGroup;
    [SerializeField] private float progressBarFadeInDuration = 0.2f;
    [SerializeField] private float progressBarFadeOutDuration = 0.2f;
    [SerializeField] private float progressBarDuration = 1.5f;
    [SerializeField] private AnimationCurve progressBarCurve;
    
    [Header("Crosshair Settings")]
    [Space(5)]
    [SerializeField] private Image crosshair;
    [SerializeField] private CanvasGroup crosshairGroup;
    [SerializeField] private float crosshairFadeInDuration = 0.2f;
    [SerializeField] private float crosshairFadeOutDuration = 0f; // Instant hide by default
    private float currentProgressBarAlpha = 0f;
    private float targetProgressBarAlpha = 0f;
    private float currentCrosshairAlpha = 1f;
    private float targetCrosshairAlpha = 1f;

    [Header("Reload Settings")]
    [Space(10)]
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private int spinAmount = 2; // How many 360 spins during reload
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private Transform weaponModel; // Reference to the actual weapon model
    
    [Header("Auto Reload Settings")]
    [Space(5)]
    [SerializeField] private bool enableAutoReload = true;
    [SerializeField] private float autoReloadDelay = 3f; // Time to wait before auto-reloading

    [Header("Fire Settings")]
    [Space(10)]
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private bool autoFire = false;
    [SerializeField] private KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] private float range = 100f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float damage = 20f;
    [SerializeField] private WeaponNetworker weaponNetworker;
    [SerializeField] private float recoilMultiplier = 1f;

    [Header("Visual Effects")]
    [Space(10)]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject damageParticlePrefab;
    [SerializeField] private Transform weaponTip;
    [SerializeField] private float muzzleFlashDuration = 1f;
    [SerializeField] private Vector3 muzzleFlashOffset;

    [Header("Audio Settings")]
    [Space(10)]
    [Header("Shoot Sound")]
    [Space(5)]
    [SerializeField] private string shootSoundName = "shoot";
    [SerializeField] private bool useRandomShootPitch = false;
    [SerializeField] private float minShootPitch = 0.9f;
    [SerializeField] private float maxShootPitch = 1.1f;
    
    [Header("Empty Magazine Sound")]
    [Space(5)]
    [SerializeField] private string emptyMagSoundName = "empty_mag";
    [SerializeField] private bool useRandomEmptyPitch = false;
    [SerializeField] private float minEmptyPitch = 0.9f;
    [SerializeField] private float maxEmptyPitch = 1.1f;
    
    [Header("Reload Sound")]
    [Space(5)]
    [SerializeField] private string reloadSoundName = "reload";
    [SerializeField] private bool useRandomReloadPitch = false;
    [SerializeField] private float minReloadPitch = 0.9f;
    [SerializeField] private float maxReloadPitch = 1.1f;

    [Header("Additional Effects")]
    [Space(10)]
    [SerializeField] private AdditionalParticle[] additionalParticles;

    private bool canShoot = true;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;
    private float reloadStartTime;
    private float emptyMagTime; // Time when magazine became empty
    private bool waitingForAutoReload = false;
    private Quaternion startRotation;
    private List<GameObject> activeParticles = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        recoilScript = GetComponentInChildren<GunRecoil>();
        currentBullets = magazineSize;
        currentMagazineSize = magazineSize; // Set view-only field
        UpdateAmmoText();
        
        // Initialize UI elements
        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(true);
            if (progressBarGroup != null)
            {
                progressBarGroup.alpha = 0f;
            }
        }
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(true);
            if (crosshairGroup != null)
            {
                crosshairGroup.alpha = 1f;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Handle UI fading
        UpdateUIFading();

        // Check for manual reload
        if (Input.GetKeyDown(reloadKey) && !isReloading && currentBullets < magazineSize)
        {
            StartReload();
        }

        // Handle auto-reload
        if (enableAutoReload && !isReloading && waitingForAutoReload)
        {
            if (Time.time >= emptyMagTime + autoReloadDelay)
            {
                StartReload();
                waitingForAutoReload = false;
            }
        }

        // Play empty mag sound if trying to shoot while reloading
        if (isReloading && Input.GetKeyDown(shootKey))
        {
            PlayEmptyMagSound();
            return;
        }

        if (isReloading)
        {
            UpdateReload();
            return;
        }

        if (canShoot)
        {
            if (Input.GetKeyDown(shootKey))
            {
                if (currentBullets <= 0)
                {
                    PlayEmptyMagSound();
                    if (enableAutoReload && !waitingForAutoReload)
                    {
                        waitingForAutoReload = true;
                        emptyMagTime = Time.time;
                    }
                    return;
                }
            }

            if (autoFire)
            {
                if (Time.time >= nextTimeToFire && Input.GetKey(shootKey) && currentBullets > 0)
                {
                    Shoot();
                    nextTimeToFire = Time.time + fireRate;
                }
            }
            else
            {
                if (Time.time >= nextTimeToFire && Input.GetKeyDown(shootKey) && currentBullets > 0)
                {
                    Shoot();
                    nextTimeToFire = Time.time + fireRate;
                }
            }
        }
    }

    void UpdateUIFading()
    {
        // Handle progress bar fading
        if (progressBarGroup != null)
        {
            float fadeSpeed = targetProgressBarAlpha > currentProgressBarAlpha ? 
                (1f / progressBarFadeInDuration) : 
                (1f / progressBarFadeOutDuration);
                
            currentProgressBarAlpha = Mathf.MoveTowards(currentProgressBarAlpha, targetProgressBarAlpha, fadeSpeed * Time.deltaTime);
            progressBarGroup.alpha = currentProgressBarAlpha;

            // Disable game object when fully transparent
            if (currentProgressBarAlpha < 0.01f && reloadProgressBar != null)
            {
                reloadProgressBar.gameObject.SetActive(false);
            }
            else if (reloadProgressBar != null && !reloadProgressBar.gameObject.activeSelf)
            {
                reloadProgressBar.gameObject.SetActive(true);
            }
        }

        // Handle crosshair fading
        if (crosshairGroup != null)
        {
            float fadeSpeed = targetCrosshairAlpha > currentCrosshairAlpha ? 
                (1f / crosshairFadeInDuration) : 
                (1f / crosshairFadeOutDuration);
                
            if (fadeSpeed != float.PositiveInfinity) // Check for instant fade (duration = 0)
            {
                currentCrosshairAlpha = Mathf.MoveTowards(currentCrosshairAlpha, targetCrosshairAlpha, fadeSpeed * Time.deltaTime);
            }
            else
            {
                currentCrosshairAlpha = targetCrosshairAlpha;
            }
            
            crosshairGroup.alpha = currentCrosshairAlpha;
        }
    }

    void Shoot()
    {
        if (currentBullets <= 0) return;

        currentBullets--;
        UpdateAmmoText();

        // Perform raycast from center of screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range, playerLayer))
        {
            // For now we just perform the raycast without any effects
            Debug.DrawLine(ray.origin, hit.point, Color.red, 1f);
            
            // Try to find HealthManager on the hit object or its parent
            HealthManager healthManager = hit.collider.GetComponentInParent<HealthManager>();
            if (healthManager != null && weaponNetworker != null)
            {
                weaponNetworker.OnPlayerHit(healthManager, damage);
            }

            // Spawn damage particles at hit point
            if (damageParticlePrefab != null)
            {
                GameObject damageParticle = Instantiate(damageParticlePrefab, hit.point, Quaternion.identity);
                DamageParticles damageParticleScript = damageParticle.GetComponent<DamageParticles>();
                if (damageParticleScript != null && damageParticleScript.damageText != null)
                {
                    damageParticleScript.damageText.text = $"-{damage}";
                }
            }
        }

        // Start auto-reload countdown when we run out of ammo
        if (currentBullets <= 0 && enableAutoReload)
        {
            waitingForAutoReload = true;
            emptyMagTime = Time.time;
            // Fade out crosshair when out of ammo
            targetCrosshairAlpha = 0f;
        }

        if (recoilScript != null)
        {
            recoilScript.Shoot(recoilMultiplier);
        }

        if (bulletPrefab != null && weaponTip != null)
        {
            Instantiate(bulletPrefab, weaponTip.position, weaponTip.rotation);
        }

        if (muzzleFlashPrefab != null && weaponTip != null)
        {
            Vector3 muzzlePosition = weaponTip.position + weaponTip.TransformDirection(muzzleFlashOffset);
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, muzzlePosition, Quaternion.identity);
            activeParticles.Add(muzzleFlash);
            Destroy(muzzleFlash, muzzleFlashDuration);
        }

        // Spawn additional particles
        if (additionalParticles != null)
        {
            foreach (AdditionalParticle particle in additionalParticles)
            {
                if (particle.particlePrefab != null)
                {
                    Transform spawnPoint = particle.positionReference != null ? particle.positionReference : weaponTip;
                    Vector3 spawnPosition = spawnPoint.position + spawnPoint.TransformDirection(particle.spawnOffset);
                    Quaternion spawnRotation = particle.useObjectRotation ? spawnPoint.rotation : particle.particlePrefab.transform.rotation;
                    
                    GameObject spawnedParticle;
                    if (particle.spawnAsChild)
                    {
                        spawnedParticle = Instantiate(particle.particlePrefab, spawnPosition, spawnRotation, spawnPoint);
                    }
                    else
                    {
                        spawnedParticle = Instantiate(particle.particlePrefab, spawnPosition, spawnRotation);
                    }
                    
                    activeParticles.Add(spawnedParticle);
                    Destroy(spawnedParticle, particle.duration);
                }
            }
        }

        PlayShootSound();
    }

    void PlayShootSound()
    {
        if (useRandomShootPitch)
        {
            AudioManager.Instance.PlayWithRandomPitch(shootSoundName, minShootPitch, maxShootPitch, false);
        }
        else
        {
            AudioManager.Instance.Play(shootSoundName, false);
        }
    }

    void PlayEmptyMagSound()
    {
        if (useRandomEmptyPitch)
        {
            AudioManager.Instance.PlayWithRandomPitch(emptyMagSoundName, minEmptyPitch, maxEmptyPitch, false);
        }
        else
        {
            AudioManager.Instance.Play(emptyMagSoundName, false);
        }
    }

    void PlayReloadSound()
    {
        if (useRandomReloadPitch)
        {
            AudioManager.Instance.PlayWithRandomPitch(reloadSoundName, minReloadPitch, maxReloadPitch, false);
        }
        else
        {
            AudioManager.Instance.Play(reloadSoundName, false);
        }
    }

    private void UpdateAmmoText()
    {
        if (currentAmmoText != null)
        {
            currentAmmoText.text = currentBullets.ToString();
        }
        if (magazineText != null)
        {
            magazineText.text = magazineSize.ToString();
        }
    }

    // Utility functions to control shooting behavior
    public void EnableShooting() => canShoot = true;
    public void DisableShooting() => canShoot = false;
    public void SetAutoFire(bool enable) => autoFire = enable;
    public void SetFireRate(float rate) => fireRate = rate;
    public void SetRecoilMultiplier(float multiplier) => recoilMultiplier = multiplier;

    void StartReload()
    {
        isReloading = true;
        waitingForAutoReload = false;
        reloadStartTime = Time.time;
        startRotation = weaponModel != null ? weaponModel.localRotation : transform.localRotation;
        canShoot = false;

        // Show progress bar and hide crosshair
        if (reloadProgressBar != null)
        {
            reloadProgressBar.gameObject.SetActive(true);
            reloadProgressBar.fillAmount = 0f;
            currentProgressBarAlpha = 0f; // Reset alpha to ensure proper fade in
            targetProgressBarAlpha = 1f;
        }
        if (crosshair != null)
        {
            targetCrosshairAlpha = 0f;
            if (crosshairGroup != null)
            {
                crosshairGroup.alpha = 0f; // Instant hide
                currentCrosshairAlpha = 0f;
            }
        }

        // Play reload sound
        PlayReloadSound();

        // Destroy all active particles
        foreach (GameObject particle in activeParticles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        activeParticles.Clear();
    }

    void UpdateReload()
    {
        float elapsedTime = Time.time - reloadStartTime;
        float reloadProgress = elapsedTime / reloadTime;
        float progressBarProgress = elapsedTime / progressBarDuration;

        // Update progress bar with its own duration and curve
        if (reloadProgressBar != null)
        {
            progressBarProgress = Mathf.Clamp01(progressBarProgress);
            reloadProgressBar.fillAmount = progressBarCurve.Evaluate(progressBarProgress);
            
            // Start fading out progress bar and fading in crosshair when progress bar is complete
            if (progressBarProgress >= 1f)
            {
                targetProgressBarAlpha = 0f;
                targetCrosshairAlpha = 1f;
            }
        }

        if (reloadProgress >= 1f)
        {
            CompleteReload();
            return;
        }

        // Calculate spin based on curve
        float curveValue = spinCurve.Evaluate(reloadProgress);
        float totalRotation = 360f * spinAmount * curveValue;
        
        // Apply rotation on X axis to the weapon model
        if (weaponModel != null)
        {
            weaponModel.localRotation = startRotation * Quaternion.Euler(totalRotation, 0, 0);
        }
    }

    void CompleteReload()
    {
        isReloading = false;
        currentBullets = magazineSize;
        UpdateAmmoText();
        canShoot = true;

        // Reset rotation to starting rotation
        if (weaponModel != null)
        {
            weaponModel.localRotation = startRotation;
        }

        // Restore crosshair visibility
        targetCrosshairAlpha = 1f;
    }

    private void OnValidate()
    {
        // Initialize curves with default values if they're null
        if (progressBarCurve == null || progressBarCurve.keys.Length == 0)
        {
            progressBarCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            progressBarCurve.preWrapMode = WrapMode.Clamp;
            progressBarCurve.postWrapMode = WrapMode.Clamp;
        }
        if (spinCurve == null || spinCurve.keys.Length == 0)
        {
            spinCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
            spinCurve.preWrapMode = WrapMode.Clamp;
            spinCurve.postWrapMode = WrapMode.Clamp;
        }
    }

    /// <summary>
    /// Disables all weapon functionality including shooting and reloading
    /// </summary>
    public void DisableWeapon()
    {
        canShoot = false;
        
        // Disable weapon sway
        if (weaponSway != null)
        {
            weaponSway.DisableSway();
        }

        // Hide UI elements
        if (crosshairGroup != null)
        {
            targetCrosshairAlpha = 0f;
        }
        if (progressBarGroup != null)
        {
            targetProgressBarAlpha = 0f;
        }

        // If currently reloading, cancel it
        if (isReloading)
        {
            isReloading = false;
            if (weaponModel != null)
            {
                weaponModel.localRotation = startRotation;
            }
        }

        // Clean up any active particles
        foreach (GameObject particle in activeParticles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        activeParticles.Clear();
    }

    /// <summary>
    /// Enables all weapon functionality including shooting and reloading
    /// </summary>
    public void EnableWeapon()
    {
        canShoot = true;
        
        // Enable weapon sway
        if (weaponSway != null)
        {
            weaponSway.EnableSway();
        }

        // Show crosshair
        if (crosshairGroup != null)
        {
            targetCrosshairAlpha = 1f;
        }

        // Reset weapon model rotation if it was in the middle of reloading
        if (weaponModel != null)
        {
            weaponModel.localRotation = startRotation;
        }
    }
}
