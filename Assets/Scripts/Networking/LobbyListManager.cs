using UnityEngine;
using Steamworks;
using System.Collections.Generic;

public class LobbyListManager : MonoBehaviour
{
    public static LobbyListManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform lobbyListContent;
    [SerializeField] private GameObject lobbyItemPrefab;

    private List<GameObject> lobbyItems = new List<GameObject>();
    protected Callback<LobbyMatchList_t> lobbyMatchList;
    protected Callback<LobbyDataUpdate_t> lobbyDataUpdate;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (!SteamManager.Initialized) return;

        lobbyMatchList = Callback<LobbyMatchList_t>.Create(OnGetLobbiesList);
        lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnGetLobbyData);
    }

    public void RefreshLobbies()
    {
        CleanLobbyList();
        SteamMatchmaking.RequestLobbyList();
    }

    private void OnGetLobbiesList(LobbyMatchList_t result)
    {
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }

    private void OnGetLobbyData(LobbyDataUpdate_t result)
    {
        if (result.m_bSuccess == 0) return;

        CSteamID lobbyID = new CSteamID(result.m_ulSteamIDLobby);
        string lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, "name");
        int currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
        int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyID);

        // Create lobby list item
        GameObject lobbyItem = Instantiate(lobbyItemPrefab, lobbyListContent);
        LobbyListItem lobbyListItem = lobbyItem.GetComponent<LobbyListItem>();
        
        if (lobbyListItem != null)
        {
            lobbyListItem.SetLobbyData(lobbyID, lobbyName, currentPlayers, maxPlayers);
            lobbyItems.Add(lobbyItem);
        }
    }

    private void CleanLobbyList()
    {
        foreach (GameObject item in lobbyItems)
        {
            Destroy(item);
        }
        lobbyItems.Clear();
    }
} 