using UnityEngine;
using Mirror;
using Steamworks;
using TMPro;
using UnityEngine.UI;

public class SteamLobby : MonoBehaviour
{
	public static SteamLobby instance;

	//Callbacks
	protected Callback<LobbyCreated_t> LobbyCreated;
	protected Callback<GameLobbyJoinRequested_t> JoinRequest;
	protected Callback<LobbyEnter_t> LobbyEntered;

	//Variables
	public ulong CurrentLobbyID;
	private const string HostAddressKey = "HostAddress";
	public CustomNetworkManager manager;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		if(!SteamManager.Initialized) { return; }

		LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
		JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
		LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
	}

	public void HostLobby()
	{
		SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, manager.maxConnections);
	}

	private void OnLobbyCreated(LobbyCreated_t callback)
	{
		if (callback.m_eResult != EResult.k_EResultOK) { return; }

		manager.StartHost();

		CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
		SteamMatchmaking.SetLobbyData(lobbyID, HostAddressKey, SteamUser.GetSteamID().ToString());
		SteamMatchmaking.SetLobbyData(lobbyID, "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
	}

	private void OnJoinRequest(GameLobbyJoinRequested_t callback)
	{
		SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
	}

	private void OnLobbyEntered(LobbyEnter_t callback)
	{
		CurrentLobbyID = callback.m_ulSteamIDLobby;

		if (NetworkServer.active) { return; }

		CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
		manager.networkAddress = SteamMatchmaking.GetLobbyData(lobbyID, HostAddressKey);
		manager.StartClient();
	}
}
