using UnityEngine;
using Mirror;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Steamworks;
using Mirror.FizzySteam;

public class CustomNetworkManager : NetworkManager
{
	[SerializeField] private PlayerObjectController GamePlayerPrefab;
	public List<PlayerObjectController> GamePlayers { get; } = new List<PlayerObjectController>();

	public override void Awake()
	{
		if (transport == null)
		{
			// Add FizzySteamworks transport if not already present
			transport = gameObject.GetComponent<FizzySteamworks>() ?? gameObject.AddComponent<FizzySteamworks>();
		}
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		if(SceneManager.GetActiveScene().name == "Lobby")
		{
			PlayerObjectController GamePlayerInstance = Instantiate(GamePlayerPrefab);

			GamePlayerInstance.ConnectionID = conn.connectionId;
			GamePlayerInstance.PlayerIdNumber = GamePlayers.Count + 1;
			GamePlayerInstance.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.CurrentLobbyID, GamePlayers.Count);

			NetworkServer.AddPlayerForConnection(conn, GamePlayerInstance.gameObject);
		}
	}

	public void StartGame(string SceneName)
	{
		ServerChangeScene(SceneName);
	}
}
