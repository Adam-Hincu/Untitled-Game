using UnityEngine;
using Mirror;

public class WeaponNetworker : NetworkBehaviour
{
    [SerializeField] private PlayerDataController playerDataController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerDataController == null)
        {
            playerDataController = GetComponent<PlayerDataController>();
        }
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
            ulong shooterId = playerDataController != null ? playerDataController.GetPlayerId() : 0;
            CmdPlayerHit(targetHealthManager.gameObject, damage, shooterId);
        }
        else
        {
            // Server hitting someone - apply damage and tell all clients
            ulong shooterId = playerDataController != null ? playerDataController.GetPlayerId() : 0;
            RpcPlayerHit(targetHealthManager.gameObject, damage, shooterId);
            ApplyDamageToPlayer(targetHealthManager, damage, shooterId);
        }
    }

    [Command]
    private void CmdPlayerHit(GameObject targetPlayer, float damage, ulong shooterId)
    {
        // Server received hit notification from client
        HealthManager healthManager = targetPlayer.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            // Apply damage and notify all clients including the original sender
            RpcPlayerHit(targetPlayer, damage, shooterId);
            ApplyDamageToPlayer(healthManager, damage, shooterId);
        }
    }

    [ClientRpc]
    private void RpcPlayerHit(GameObject targetPlayer, float damage, ulong shooterId)
    {
        // Skip if we're the server since the server already applied damage
        if (isServer) return;

        HealthManager healthManager = targetPlayer.GetComponent<HealthManager>();
        if (healthManager != null)
        {
            ApplyDamageToPlayer(healthManager, damage, shooterId);
        }
    }

    private void ApplyDamageToPlayer(HealthManager healthManager, float damage, ulong shooterId)
    {
        healthManager.TakeDamage(damage, shooterId);
    }
}
