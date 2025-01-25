using UnityEngine;
using TMPro;
using Steamworks;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private Button joinButton;

    private CSteamID lobbyID;
    private string lobbyName;
    private int playerCount;
    private int maxPlayers;

    private void Start()
    {
        if (joinButton != null)
            joinButton.onClick.AddListener(JoinLobby);
    }

    public void SetLobbyData(CSteamID lobbyId, string name, int currentPlayers, int maxPlayerCount)
    {
        lobbyID = lobbyId;
        lobbyName = name;
        playerCount = currentPlayers;
        maxPlayers = maxPlayerCount;

        if (lobbyNameText != null)
            lobbyNameText.text = lobbyName;
        
        if (playerCountText != null)
            playerCountText.text = $"{playerCount}/{maxPlayers}";
    }

    private void JoinLobby()
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }
} 