using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth = 100f;
    
    [Header("Regeneration Settings")]
    [SerializeField] private float timeToStartRegen = 5f;
    [SerializeField] private float regenAmountPerSecond = 15f;
    private float timeSinceLastDamage;
    private float regenTickTimer;
    
    [Header("UI References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private RectTransform healthBarHolder;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Animation Settings")]
    [SerializeField, Tooltip("Duration of the animation in seconds")] 
    private float healthBarAnimationDuration = 0.2f;
    [SerializeField] private AnimationCurve healthBarAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Color Settings")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color healColor = Color.cyan;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField, Tooltip("Duration of the color flash in seconds")] 
    private float colorFlashDuration = 0.5f;

    [Header("Text Color Settings")]
    [SerializeField] private Color textNormalColor = Color.white;
    [SerializeField] private Color textHealColor = Color.cyan;
    [SerializeField] private Color textDamageColor = Color.red;
    
    [Header("Low Health State Settings")]
    [SerializeField] private float lowHealthThreshold = 30f;
    [SerializeField] private Color lowHealthTextColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private float lowHealthColorTransitionDuration = 0.5f;
    private bool isInLowHealthState = false;
    private Coroutine lowHealthTransitionCoroutine;
    
    [Header("Scale Animation Settings")]
    [SerializeField] private Vector3 normalScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private Vector3 damageScalePunch = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField, Tooltip("Duration of the scale punch in seconds")]
    private float scalePunchDuration = 0.3f;
    [SerializeField] private AnimationCurve scalePunchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Network References")]
    public PlayerDataController playerDataController;
    
    private float targetHealthFill;
    private Coroutine healthBarAnimationCoroutine;
    private Coroutine colorAnimationCoroutine;
    private Coroutine scaleAnimationCoroutine;

    [Header("Text Animation Settings")]
    [SerializeField] private float textAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve textAnimationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    private float displayedHealth;
    private Coroutine textAnimationCoroutine;

    [Header("Death Screen Reference")]
    [SerializeField] private DeathScreenController deathScreenController;

    private ulong lastShooterId;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        displayedHealth = currentHealth;
        if (healthBarHolder != null)
        {
            healthBarHolder.localScale = normalScale;
        }
        UpdateHealthUI();
        
        timeSinceLastDamage = 0f;

        // Wait a frame to ensure PlayerDataController is properly initialized
        StartCoroutine(InitialHealthSync());
    }

    private IEnumerator InitialHealthSync()
    {
        // Wait for next frame to ensure all components are initialized
        yield return new WaitForEndOfFrame();
        
        // Make sure we have the PlayerDataController reference
        if (playerDataController == null)
        {
            playerDataController = GetComponent<PlayerDataController>();
        }
        
        // Sync initial health to PlayerDataController
        if (playerDataController != null)
        {
            SyncHealthToNetwork(maxHealth);
        }
    }

    private void SyncHealthToNetwork(float health)
    {
        if (playerDataController != null)
        {
            // Only sync if we're on the server
            if (playerDataController.isServer)
            {
                playerDataController.ServerUpdateHealth(health);
            }
            else
            {
                // If we're not on the server, we shouldn't try to sync
                // The server will handle health changes through WeaponNetworker
                Debug.LogWarning("Attempted to sync health from client. Health changes should be handled by the server.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Temporary debug controls
        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(15f);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(15f);
        }

        // Handle regeneration
        if (currentHealth < maxHealth)
        {
            timeSinceLastDamage += Time.deltaTime;
            
            if (timeSinceLastDamage >= timeToStartRegen)
            {
                regenTickTimer += Time.deltaTime;
                
                // Regenerate health every second
                if (regenTickTimer >= 1f)
                {
                    Heal(regenAmountPerSecond);
                    regenTickTimer = 0f;
                }
            }
        }
    }

    public void KillPlayer()
    {
        currentHealth = 0f;
        
        // Sync to network immediately
        SyncHealthToNetwork(currentHealth);
        // Update visuals
        SetHealthFromSync(currentHealth);
        
        // Call the death screen controller's Kill function with killer ID
        if (deathScreenController != null)
        {
            deathScreenController.Kill(lastShooterId);
        }
        else
        {
            Debug.LogError("DeathScreenController reference is missing in HealthManager!");
        }
    }

    public void Revive()
    {
        currentHealth = maxHealth;
        // Sync to network immediately
        SyncHealthToNetwork(currentHealth);
        // Update visuals
        SetHealthFromSync(currentHealth);
    }

    public void TakeDamage(float damageAmount, ulong shooterId)
    {
        // Don't take damage if already dead
        if (currentHealth <= 0f)
            return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Max(0f, currentHealth - damageAmount);
        
        // Store the shooter's ID
        lastShooterId = shooterId;
        
        // Sync to network immediately
        SyncHealthToNetwork(currentHealth);
        
        // Check if player died
        if (currentHealth <= 0f)
        {
            KillPlayer();
        }
        
        // Visual updates - only if we have UI components
        if (healthFillImage != null)
        {
            UpdateHealthUI();
            FlashHealthBarColor(damageColor);
            PunchScale();
        }
        
        // Reset regeneration timer when taking damage
        timeSinceLastDamage = 0f;
        regenTickTimer = 0f;
    }

    // Keep the old method for compatibility with any other code that might use it
    public void TakeDamage(float damageAmount)
    {
        TakeDamage(damageAmount, 0);
    }

    public void Heal(float healAmount)
    {
        // Don't heal if dead or already at max health
        if (currentHealth <= 0f || currentHealth >= maxHealth)
            return;

        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        
        // Sync to network immediately
        SyncHealthToNetwork(currentHealth);
        
        // Visual updates - only if we have UI components
        if (healthFillImage != null)
        {
            UpdateHealthUI();
            FlashHealthBarColor(healColor);
        }
    }

    public void SetHealthFromSync(float newHealth)
    {
        float previousHealth = currentHealth;
        currentHealth = newHealth;
        
        // Visual updates - only if we have UI components
        if (healthFillImage != null)
        {
            UpdateHealthUI();
            
            if (currentHealth > previousHealth)
            {
                FlashHealthBarColor(healColor);
            }
            else if (currentHealth < previousHealth)
            {
                FlashHealthBarColor(damageColor);
                PunchScale();
            }
        }
    }

    private void FlashHealthBarColor(Color flashColor)
    {
        // Only proceed if we have the required UI components
        if (healthFillImage == null) return;
        
        if (colorAnimationCoroutine != null)
        {
            StopCoroutine(colorAnimationCoroutine);
        }
        
        // Only animate the text color if we're not in low health state and have a health text component
        if (!isInLowHealthState && healthText != null)
        {
            colorAnimationCoroutine = StartCoroutine(AnimateHealthBarColor(flashColor));
        }
        else
        {
            // If in low health state or no text component, only animate the health bar
            colorAnimationCoroutine = StartCoroutine(AnimateOnlyHealthBar(flashColor));
        }
    }

    private IEnumerator AnimateHealthBarColor(Color targetColor)
    {
        float elapsedTime = 0f;
        Color startBarColor = healthFillImage.color;
        Color startTextColor = healthText != null ? healthText.color : textNormalColor;
        
        // Determine which text color to use based on the target color
        Color targetTextColor = targetColor == healColor ? textHealColor : 
                               targetColor == damageColor ? textDamageColor : 
                               textNormalColor;
        
        // Fade to target color
        while (elapsedTime < colorFlashDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (colorFlashDuration * 0.5f);
            healthFillImage.color = Color.Lerp(startBarColor, targetColor, t);
            if (healthText != null)
            {
                healthText.color = Color.Lerp(startTextColor, targetTextColor, t);
            }
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Fade back to normal
        while (elapsedTime < colorFlashDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (colorFlashDuration * 0.5f);
            healthFillImage.color = Color.Lerp(targetColor, normalColor, t);
            if (healthText != null)
            {
                healthText.color = Color.Lerp(targetTextColor, textNormalColor, t);
            }
            yield return null;
        }
        
        healthFillImage.color = normalColor;
        if (healthText != null)
        {
            healthText.color = textNormalColor;
        }
    }

    private void UpdateHealthUI()
    {
        // Update health bar fill
        if (healthFillImage != null)
        {
            targetHealthFill = currentHealth / maxHealth;
            
            if (healthBarAnimationCoroutine != null)
            {
                StopCoroutine(healthBarAnimationCoroutine);
            }
            
            healthBarAnimationCoroutine = StartCoroutine(AnimateHealthBar());
        }

        // Update health text with animation
        if (healthText != null)
        {
            if (textAnimationCoroutine != null)
            {
                StopCoroutine(textAnimationCoroutine);
            }
            textAnimationCoroutine = StartCoroutine(AnimateHealthText());
            CheckLowHealthState();
        }
    }
    
    private IEnumerator AnimateHealthBar()
    {
        float startFill = healthFillImage.fillAmount;
        float elapsedTime = 0f;
        
        while (elapsedTime < healthBarAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float curveValue = healthBarAnimationCurve.Evaluate(elapsedTime / healthBarAnimationDuration);
            healthFillImage.fillAmount = Mathf.Lerp(startFill, targetHealthFill, curveValue);
            yield return null;
        }
        
        healthFillImage.fillAmount = targetHealthFill;
    }

    private IEnumerator AnimateHealthText()
    {
        float startHealth = displayedHealth;
        float targetHealth = currentHealth;
        float elapsedTime = 0f;
        
        while (elapsedTime < textAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = textAnimationCurve.Evaluate(elapsedTime / textAnimationDuration);
            displayedHealth = Mathf.Lerp(startHealth, targetHealth, t);
            
            // Update the text with the current interpolated value
            if (healthText != null)
            {
                healthText.text = $"{Mathf.CeilToInt(displayedHealth)}/{Mathf.CeilToInt(maxHealth)}";
            }
            
            yield return null;
        }
        
        // Ensure we end up at the exact target value
        displayedHealth = targetHealth;
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(displayedHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void PunchScale()
    {
        if (healthBarHolder != null && currentHealth > 0)
        {
            if (scaleAnimationCoroutine != null)
            {
                StopCoroutine(scaleAnimationCoroutine);
            }
            scaleAnimationCoroutine = StartCoroutine(AnimateScale());
        }
    }

    private IEnumerator AnimateScale()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < scalePunchDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / scalePunchDuration;
            
            // Use the animation curve directly for the entire motion
            float curveValue = scalePunchCurve.Evaluate(t);
            // Convert the 0-1 curve into a punch effect (0->1->0)
            float punchAmount = 1f - (curveValue * 2f - 1f) * (curveValue * 2f - 1f);
            Vector3 targetScale = Vector3.Lerp(normalScale, damageScalePunch, punchAmount);
            
            healthBarHolder.localScale = targetScale;
            yield return null;
        }
        
        healthBarHolder.localScale = normalScale;
    }

    private void CheckLowHealthState()
    {
        bool shouldBeLowHealth = currentHealth <= lowHealthThreshold;
        
        if (shouldBeLowHealth != isInLowHealthState)
        {
            isInLowHealthState = shouldBeLowHealth;
            
            if (lowHealthTransitionCoroutine != null)
            {
                StopCoroutine(lowHealthTransitionCoroutine);
            }
            
            lowHealthTransitionCoroutine = StartCoroutine(TransitionToLowHealthState(shouldBeLowHealth));
        }
    }

    private IEnumerator TransitionToLowHealthState(bool enteringLowHealth)
    {
        Color startColor = healthText.color;
        Color targetColor = enteringLowHealth ? lowHealthTextColor : textNormalColor;
        float elapsedTime = 0f;

        while (elapsedTime < lowHealthColorTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / lowHealthColorTransitionDuration;
            healthText.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        healthText.color = targetColor;
    }

    private IEnumerator AnimateOnlyHealthBar(Color targetColor)
    {
        float elapsedTime = 0f;
        Color startBarColor = healthFillImage.color;
        
        // Fade to target color
        while (elapsedTime < colorFlashDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (colorFlashDuration * 0.5f);
            healthFillImage.color = Color.Lerp(startBarColor, targetColor, t);
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Fade back to normal
        while (elapsedTime < colorFlashDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (colorFlashDuration * 0.5f);
            healthFillImage.color = Color.Lerp(targetColor, normalColor, t);
            yield return null;
        }
        
        healthFillImage.color = normalColor;
    }
}
