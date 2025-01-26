using UnityEngine;
using Mirror;

public class WeaponNetworker : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerHit(HealthManager targetHealthManager, float damage)
    {
        // Only the server should process the hit to avoid duplicate damage
        if (!isServer)
        {
            // If we're not the server, tell the server about the hit
            CmdPlayerHit(targetHealthManager.gameObject, damage);
            return;
        }

        // If we are the server, apply damage directly
        ApplyDamageToPlayer(targetHealthManager, damage);
    }

    [Command]
    private void CmdPlayerHit(GameObject targetPlayer, float damage)
    {
        // Server received hit notification from client
        HealthManager healthManager = targetPlayer.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            ApplyDamageToPlayer(healthManager, damage);
        }
    }

    private void ApplyDamageToPlayer(HealthManager healthManager, float damage)
    {
        // Apply damage and notify all clients about the health change
        healthManager.TakeDamage(damage);
    }
}
