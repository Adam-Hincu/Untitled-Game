using UnityEngine;

public class DeathScreenController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthManager healthManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private WeaponHolder weaponHolder;

    [Header("Death Settings")]
    [SerializeField] private float deathTime = 5f;
    private float currentDeathTimer;
    private bool isDead = false;

    public ulong killerId { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            currentDeathTimer -= Time.deltaTime;
            
            if (currentDeathTimer <= 0)
            {
                RevivePlayer();
            }
        }
    }

    public void Kill(ulong killerPlayerId = 0)
    {
        isDead = true;
        currentDeathTimer = deathTime;
        
        // Store killer information
        killerId = killerPlayerId;

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
            isDead = false;
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
