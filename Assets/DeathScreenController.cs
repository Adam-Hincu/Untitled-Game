using UnityEngine;

public class DeathScreenController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthManager healthManager;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Weapon weapon;

    [Header("Death Settings")]
    [SerializeField] private float deathTime = 5f;
    private float currentDeathTimer;
    private bool isDead = false;

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

    public void Kill()
    {
        Debug.Log("You died");
        isDead = true;
        currentDeathTimer = deathTime;

        // Disable player movement when dead
        if (playerMovement != null)
        {
            playerMovement.DisableAllControls();
        }
        else
        {
            Debug.LogError("PlayerMovement reference is missing in DeathScreenController!");
        }

        // Disable weapon when dead
        if (weapon != null)
        {
            //weapon.DisableWeapon();
        }
        else
        {
            Debug.LogError("Weapon reference is missing in DeathScreenController!");
        }
    }

    private void RevivePlayer()
    {
        if (healthManager != null)
        {
            isDead = false;
            healthManager.Revive();
            Debug.Log("You have been revived!");

            // Re-enable player movement when revived
            if (playerMovement != null)
            {
                playerMovement.EnableAllControls();
            }

            // Re-enable weapon when revived
            if (weapon != null)
            {
                //weapon.EnableWeapon();
            }
        }
        else
        {
            Debug.LogError("HealthManager reference is missing in DeathScreenController!");
        }
    }
}
