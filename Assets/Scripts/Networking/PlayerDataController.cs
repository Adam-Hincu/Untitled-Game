using UnityEngine;
using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlayerData
{
    public ulong steamId;
    public string playerName;
    public Sprite avatar;
    public float currentHealth;
}

public class PlayerDataController : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(OnSteamIdUpdated)), SerializeField, ReadOnly]
    private ulong playerSteamId;
    [SyncVar, SerializeField, ReadOnly]
    private string playerName;
    [SyncVar(hook = nameof(OnHealthChanged)), SerializeField, ReadOnly]
    private float currentHealth;
    
    [SerializeField, ReadOnly]
    private Sprite playerAvatar;
    private Texture2D avatarTexture;

    private HealthManager healthManager;

    [Header("Other Players")]
    [SerializeField, ReadOnly]
    private List<PlayerData> otherPlayers = new List<PlayerData>();

    private void UpdateOtherPlayersList()
    {
        if (!isLocalPlayer) return;

        // Clear old list
        otherPlayers.Clear();

        // Find all PlayerDataControllers in the scene
        var allPlayers = FindObjectsByType<PlayerDataController>(FindObjectsSortMode.None);
        
        foreach (var player in allPlayers)
        {
            // Skip if it's our own controller
            if (player == this || player.playerSteamId == 0) continue;

            otherPlayers.Add(new PlayerData
            {
                steamId = player.playerSteamId,
                playerName = player.playerName,
                avatar = player.playerAvatar,
                currentHealth = player.currentHealth
            });
        }
    }

    public override void OnStartClient()
    {
        healthManager = GetComponent<HealthManager>();
        if (!isLocalPlayer) 
        {
            if (playerSteamId != 0)
                UpdateAvatarFromSteamId();
            return;
        }
        
        if (!SteamManager.Initialized) return;

        // Get and sync Steam ID
        playerSteamId = SteamUser.GetSteamID().m_SteamID;
        
        // Get and sync player name
        playerName = SteamFriends.GetPersonaName();
        
        // Get local avatar
        UpdateAvatarFromSteamId();
        
        // Tell the server to sync our data
        CmdSyncPlayerData(playerSteamId, playerName);

        // Start updating other players list - now every 0.1 seconds for more responsive updates
        InvokeRepeating(nameof(UpdateOtherPlayersList), 0.1f, 0.1f);
    }

    public override void OnStopClient()
    {
        if (isLocalPlayer)
        {
            CancelInvoke(nameof(UpdateOtherPlayersList));
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        if (playerSteamId != 0)
            UpdateAvatarFromSteamId();
    }

    private void OnSteamIdUpdated(ulong oldSteamId, ulong newSteamId)
    {
        if (newSteamId != 0)
            UpdateAvatarFromSteamId();
    }

    private void OnHealthChanged(float oldHealth, float newHealth)
    {
        if (healthManager != null)
        {
            healthManager.SetHealthFromSync(newHealth);
        }
    }

    [Command]
    private void CmdSyncPlayerData(ulong steamId, string name)
    {
        playerSteamId = steamId;
        playerName = name;
    }

    [Command]
    public void CmdUpdateHealth(float newHealth)
    {
        currentHealth = newHealth;
        // Immediately notify all clients of the health change
        RpcUpdateHealth(newHealth);
    }

    [ClientRpc]
    private void RpcUpdateHealth(float newHealth)
    {
        // Update the health value immediately on all clients
        currentHealth = newHealth;
        
        // If this is not the local player and we have a health manager, update it
        if (!isLocalPlayer && healthManager != null)
        {
            healthManager.SetHealthFromSync(newHealth);
        }
    }

    private void UpdateAvatarFromSteamId()
    {
        if (playerSteamId == 0 || !SteamManager.Initialized) return;

        int avatarInt = SteamFriends.GetLargeFriendAvatar((CSteamID)playerSteamId);
        if (avatarInt > 0)
        {
            uint width, height;
            bool success = SteamUtils.GetImageSize(avatarInt, out width, out height);
            if (success)
            {
                byte[] avatarData = new byte[width * height * 4];
                success = SteamUtils.GetImageRGBA(avatarInt, avatarData, (int)(width * height * 4));
                if (success)
                {
                    if (avatarTexture == null)
                        avatarTexture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    
                    avatarTexture.LoadRawTextureData(avatarData);
                    avatarTexture.Apply();
                    
                    if (playerAvatar != null)
                        Destroy(playerAvatar);
                        
                    playerAvatar = Sprite.Create(avatarTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (avatarTexture != null)
            Destroy(avatarTexture);
        if (playerAvatar != null)
            Destroy(playerAvatar);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

// Attribute to make fields read-only in inspector
public class ReadOnlyAttribute : PropertyAttribute { }
