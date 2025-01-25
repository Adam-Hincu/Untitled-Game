using UnityEngine;
using Mirror;
using Steamworks;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviour
{
	public static LobbyController instance;

	[Header("UI References")]
	[SerializeField] private TMP_Text lobbyNameText = null;
	[SerializeField] private GameObject playerListContainer;
	[SerializeField] private GameObject playerListItemPrefab;

	[Header("Game Settings")]
#if UNITY_EDITOR
	[SerializeField] private UnityEditor.SceneAsset gameScene;
#endif
	[SerializeField] private string gameSceneName;

	// Local Player Reference
	private PlayerObjectController _localPlayerController;
	public PlayerObjectController localPlayerController => _localPlayerController;

	// Private fields
	private GameObject localPlayerObject;
	private bool isPlayerListInitialized;
	private List<PlayerListItem> playerListItems = new List<PlayerListItem>();
	private ulong currentLobbyID;
	private CustomNetworkManager networkManager;

	private CustomNetworkManager NetworkManager
	{
		get
		{
			if (networkManager != null)
			{
				return networkManager;
			}
			return networkManager = Mirror.NetworkManager.singleton as CustomNetworkManager;
		}
	}

	private void Awake()
	{
		instance = this;
	}

	public void UpdateLobbyName()
	{
		currentLobbyID = NetworkManager.GetComponent<SteamLobby>().CurrentLobbyID;
		lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(currentLobbyID), "name");
	}

	public void UpdatePlayerList()
	{
		if (!gameObject.activeInHierarchy) return;

		if(!isPlayerListInitialized)
		{
			InitializeHostPlayerList();
		}

		if(playerListItems.Count < NetworkManager.GamePlayers.Count)
		{
			AddNewPlayerToList();
		}

		if (playerListItems.Count > NetworkManager.GamePlayers.Count)
		{
			RemoveDisconnectedPlayers();
		}

		if(playerListItems.Count == NetworkManager.GamePlayers.Count)
		{
			RefreshPlayerList();
		}
	}

	public void FindLocalPlayer()
	{
		localPlayerObject = GameObject.Find("LocalGamePlayer");
		_localPlayerController = localPlayerObject.GetComponent<PlayerObjectController>();
	}

	private void InitializeHostPlayerList()
	{
		foreach(PlayerObjectController player in NetworkManager.GamePlayers)
		{
			GameObject newPlayerItem = Instantiate(playerListItemPrefab);
			PlayerListItem playerListItem = newPlayerItem.GetComponent<PlayerListItem>();

			playerListItem.PlayerName = player.PlayerName;
			playerListItem.ConnectionID = player.ConnectionID;
			playerListItem.PlayerSteamID = player.PlayerSteamID;
			playerListItem.SetPlayerValues();

			newPlayerItem.transform.SetParent(playerListContainer.transform);
			newPlayerItem.transform.localScale = Vector3.one;

			playerListItems.Add(playerListItem);
		}

		isPlayerListInitialized = true;
	}

	private void AddNewPlayerToList()
	{
		foreach (PlayerObjectController player in NetworkManager.GamePlayers)
		{
			if (!playerListItems.Any(item => item.ConnectionID == player.ConnectionID))
			{
				GameObject newPlayerItem = Instantiate(playerListItemPrefab);
				PlayerListItem playerListItem = newPlayerItem.GetComponent<PlayerListItem>();

				playerListItem.PlayerName = player.PlayerName;
				playerListItem.ConnectionID = player.ConnectionID;
				playerListItem.PlayerSteamID = player.PlayerSteamID;
				playerListItem.SetPlayerValues();

				newPlayerItem.transform.SetParent(playerListContainer.transform);
				newPlayerItem.transform.localScale = Vector3.one;

				playerListItems.Add(playerListItem);
			}
		}
	}

	private void RefreshPlayerList()
	{
		foreach (PlayerObjectController player in NetworkManager.GamePlayers)
		{
			foreach (PlayerListItem playerListItem in playerListItems)
			{
				if(playerListItem.ConnectionID == player.ConnectionID)
				{
					playerListItem.PlayerName = player.PlayerName;
					playerListItem.SetPlayerValues();
				}
			}
		}
	}

	private void RemoveDisconnectedPlayers()
	{
		// Create a new list to avoid modification during enumeration
		List<PlayerListItem> playersToRemove = new List<PlayerListItem>();

		// Filter out null entries and find disconnected players
		playerListItems.RemoveAll(item => item == null || item.gameObject == null);

		foreach (PlayerListItem playerListItem in playerListItems)
		{
			if (playerListItem != null && !NetworkManager.GamePlayers.Any(player => player.ConnectionID == playerListItem.ConnectionID))
			{
				playersToRemove.Add(playerListItem);
			}
		}

		if(playersToRemove.Count > 0)
		{
			foreach(PlayerListItem playerToRemove in playersToRemove)
			{
				if (playerToRemove != null && playerToRemove.gameObject != null)
				{
					GameObject itemToRemove = playerToRemove.gameObject;
					playerListItems.Remove(playerToRemove);
					Destroy(itemToRemove);
				}
			}
		}
	}

	public void StartGame()
	{
		if (!NetworkManager.isNetworkActive || !NetworkServer.active)
			return;

		// Clear the player list before changing scenes
		foreach (var item in playerListItems)
		{
			if (item != null && item.gameObject != null)
				Destroy(item.gameObject);
		}
		playerListItems.Clear();
		isPlayerListInitialized = false;

		NetworkManager.ServerChangeScene(gameSceneName);
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (gameScene != null)
		{
			gameSceneName = gameScene.name;
		}
	}
#endif
}
