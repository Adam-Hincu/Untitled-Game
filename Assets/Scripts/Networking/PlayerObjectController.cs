using UnityEngine;
using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
	//Player Data
	[SyncVar] public int ConnectionID;
	[SyncVar] public int PlayerIdNumber;
	[SyncVar] public ulong PlayerSteamID;
	[SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;

	private CustomNetworkManager manager;

	private CustomNetworkManager Manager
	{
		get
		{
			if(manager != null)
			{
				return manager;
			}
			return manager = NetworkManager.singleton as CustomNetworkManager;
		}
	}

	public override void OnStartAuthority()
	{
		DontDestroyOnLoad(gameObject);
		CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
		gameObject.name = "LocalGamePlayer";
		LobbyController.instance.FindLocalPlayer();
		LobbyController.instance.UpdateLobbyName();
	}

	public override void OnStartClient()
	{
		Manager.GamePlayers.Add(this);
		LobbyController.instance.UpdateLobbyName();
		LobbyController.instance.UpdatePlayerList();
	}

	public override void OnStopClient()
	{
		Manager.GamePlayers.Remove(this);
		if (LobbyController.instance != null && LobbyController.instance.gameObject != null && LobbyController.instance.gameObject.activeInHierarchy)
		{
			LobbyController.instance.UpdatePlayerList();
		}
	}

	[Command]
	private void CmdSetPlayerName(string PlayerName)
	{
		this.PlayerNameUpdate(this.PlayerName, PlayerName);
	}

	public void PlayerNameUpdate(string OldValue, string NewValue)
	{
		if(isServer)
		{
			this.PlayerName = NewValue;
		}
		if(isClient)
		{
			LobbyController.instance.UpdatePlayerList();
		}
	}

	private void Start()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	//Start Game
	public void CanStartGame(string SceneName)
	{
		if (isOwned)
		{
			CmdCanStartGame(SceneName);
		}
	}

	[Command]
	public void CmdCanStartGame(string SceneName)
	{
		manager.StartGame(SceneName);
	}
}
