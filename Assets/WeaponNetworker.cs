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
        if (!isServer)
        {
            // Client hitting someone - tell the server
            CmdPlayerHit(targetHealthManager.gameObject, damage);
        }
        else
        {
            // Server hitting someone - apply damage and tell all clients
            RpcPlayerHit(targetHealthManager.gameObject, damage);
            ApplyDamageToPlayer(targetHealthManager, damage);
        }
    }

    [Command]
    private void CmdPlayerHit(GameObject targetPlayer, float damage)
    {
        // Server received hit notification from client
        HealthManager healthManager = targetPlayer.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            // Apply damage and notify all clients including the original sender
            RpcPlayerHit(targetPlayer, damage);
            ApplyDamageToPlayer(healthManager, damage);
        }
    }

    [ClientRpc]
    private void RpcPlayerHit(GameObject targetPlayer, float damage)
    {
        // Skip if we're the server since the server already applied damage
        if (isServer) return;

        HealthManager healthManager = targetPlayer.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            ApplyDamageToPlayer(healthManager, damage);
        }
    }

    private void ApplyDamageToPlayer(HealthManager healthManager, float damage)
    {
        healthManager.TakeDamage(damage);
    }
}
