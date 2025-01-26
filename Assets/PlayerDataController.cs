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
}

public class PlayerDataController : NetworkBehaviour
{
    [Header("Player Data")]
    [SyncVar(hook = nameof(OnSteamIdUpdated)), SerializeField, ReadOnly]
    private ulong playerSteamId;
    [SyncVar, SerializeField, ReadOnly]
    private string playerName;
    
    [SerializeField, ReadOnly]
    private Sprite playerAvatar;
    private Texture2D avatarTexture;

    [Header("Other Players")]
    [SerializeField, ReadOnly]
    private List<PlayerData> otherPlayers = new List<PlayerData>();

    public override void OnStartClient()
    {
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

        // Start tracking other players
        InvokeRepeating(nameof(UpdateOtherPlayersList), 1f, 1f);
    }

    private void UpdateOtherPlayersList()
    {
        if (!isLocalPlayer) return;

        // Clear old list
        foreach (var player in otherPlayers)
        {
            if (player.avatar != null)
                Destroy(player.avatar);
        }
        otherPlayers.Clear();

        // Find all player controllers
        var allPlayers = FindObjectsOfType<PlayerDataController>();
        foreach (var player in allPlayers)
        {
            // Skip if it's our own data or if the player has no Steam ID yet
            if (player == this || player.playerSteamId == 0) continue;

            // Create new player data
            PlayerData playerData = new PlayerData
            {
                steamId = player.playerSteamId,
                playerName = player.playerName,
                avatar = player.playerAvatar
            };

            otherPlayers.Add(playerData);
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

    [Command]
    private void CmdSyncPlayerData(ulong steamId, string name)
    {
        playerSteamId = steamId;
        playerName = name;
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
            
        // Clean up other players' avatars
        foreach (var player in otherPlayers)
        {
            if (player.avatar != null)
                Destroy(player.avatar);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

// Attribute to make fields read-only in inspector
public class ReadOnlyAttribute : PropertyAttribute { }
