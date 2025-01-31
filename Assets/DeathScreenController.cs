using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class DeathScreenController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthManager healthManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private WeaponHolder weaponHolder;
    [SerializeField] private PlayerDataController playerDataController;

    [Header("UI References")]
    [SerializeField] private CanvasGroup deathScreenCanvasGroup;
    [SerializeField] private CanvasGroup gameUICanvasGroup;
    [SerializeField] private TextMeshProUGUI killerNameText;
    [SerializeField] private RawImage killerProfileImage;

    [Header("Death Settings")]
    [SerializeField] private float deathTime = 5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private float currentDeathTimer;
    private float currentFadeTimer;
    private bool isDead = false;
    private bool isFadingIn = false;
    private bool isFadingOut = false;

    public ulong killerId { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize death screen to be invisible and disabled at start
        if (deathScreenCanvasGroup != null)
        {
            deathScreenCanvasGroup.alpha = 0;
            deathScreenCanvasGroup.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            if (isFadingIn)
            {
                if (!deathScreenCanvasGroup.gameObject.activeSelf)
                {
                    deathScreenCanvasGroup.gameObject.SetActive(true);
                }
                
                currentFadeTimer += Time.deltaTime;
                float progress = currentFadeTimer / fadeInDuration;
                
                deathScreenCanvasGroup.alpha = Mathf.Lerp(0, 1, progress);
                gameUICanvasGroup.alpha = Mathf.Lerp(1, 0, progress);

                if (progress >= 1)
                {
                    isFadingIn = false;
                    deathScreenCanvasGroup.alpha = 1;
                    gameUICanvasGroup.alpha = 0;
                }
            }
            else if (isFadingOut)
            {
                currentFadeTimer += Time.deltaTime;
                float progress = currentFadeTimer / fadeOutDuration;
                
                deathScreenCanvasGroup.alpha = Mathf.Lerp(1, 0, progress);
                gameUICanvasGroup.alpha = Mathf.Lerp(0, 1, progress);

                if (progress >= 1)
                {
                    isFadingOut = false;
                    deathScreenCanvasGroup.alpha = 0;
                    gameUICanvasGroup.alpha = 1;
                    isDead = false;
                    deathScreenCanvasGroup.gameObject.SetActive(false);
                }
            }
            else
            {
                currentDeathTimer -= Time.deltaTime;
                
                if (currentDeathTimer <= 0)
                {
                    StartFadeOut();
                }
            }
        }
    }

    private void StartFadeIn()
    {
        isFadingIn = true;
        isFadingOut = false;
        currentFadeTimer = 0;
    }

    private void StartFadeOut()
    {
        isFadingIn = false;
        isFadingOut = true;
        currentFadeTimer = 0;
        RevivePlayer();
    }

    public void Kill(ulong killerPlayerId = 0)
    {
        isDead = true;
        currentDeathTimer = deathTime;
        killerId = killerPlayerId;

        // Update killer information if we have a valid killer ID
        if (killerId != 0 && playerDataController != null)
        {
            var allPlayers = FindObjectsByType<PlayerDataController>(FindObjectsSortMode.None);
            var killerData = allPlayers.FirstOrDefault(p => p.GetPlayerId() == killerId);
            
            if (killerData != null)
            {
                killerNameText.text = killerData.GetPlayerName();
                if (killerProfileImage != null)
                {
                    var killerAvatar = killerData.GetPlayerAvatar();
                    if (killerAvatar != null)
                    {
                        killerProfileImage.texture = killerAvatar.texture;
                    }
                }
            }
        }
        else
        {
            // Handle environment kill - no killer profile
            killerNameText.text = "Environment";
            if (killerProfileImage != null)
            {
                killerProfileImage.gameObject.SetActive(false);
            }
        }

        // Start the fade in transition
        StartFadeIn();

        // Disable player movement when dead
        if (playerMovement != null)
        {
            playerMovement.DisableAllControls();
        }
        else
        {
            Debug.LogError("PlayerMovement reference is missing in DeathScreenController!");
        }

        // Disable weapons when dead
        if (weaponHolder != null)
        {
            weaponHolder.DisableWeapons();
        }
        else
        {
            Debug.LogError("WeaponHolder reference is missing in DeathScreenController!");
        }
    }

    private void RevivePlayer()
    {
        if (healthManager != null)
        {
            healthManager.Revive();

            // Re-enable player movement when revived
            if (playerMovement != null)
            {
                playerMovement.EnableAllControls();
            }

            // Re-enable weapons when revived
            if (weaponHolder != null)
            {
                weaponHolder.EnableWeapons();
            }
        }
        else
        {
            Debug.LogError("HealthManager reference is missing in DeathScreenController!");
        }
    }
}
