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
            ApplyDamageToPlayer(targetHealthManager, damage);
            RpcPlayerHit(targetHealthManager.gameObject, damage);
        }
    }

    [Command]
    private void CmdPlayerHit(GameObject targetPlayer, float damage)
    {
        // Server received hit notification from client
        HealthManager healthManager = targetPlayer.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            // Apply damage on server and notify all clients except the server
            ApplyDamageToPlayer(healthManager, damage);
            RpcPlayerHit(targetPlayer, damage);
        }
    }

    [ClientRpc]
    private void RpcPlayerHit(GameObject targetPlayer, float damage)
    {
        // Only apply damage on non-server clients
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
