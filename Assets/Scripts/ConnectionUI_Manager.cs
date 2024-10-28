using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Networking.Transport.Relay;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;

public class ConnectionUI_Manager : MonoBehaviour
{
    public static ConnectionUI_Manager Instance { get; private set; } = null;
    private FacepunchTransport transport;
	public Lobby? CurrentLobby { get; private set; } = null;
    public List<Lobby> Lobbies { get; private set; } = new List<Lobby>(capacity: 100);

    [SerializeField]
    private GameObject panel;
    [SerializeField]
    private Draft_Manager draftManager;

    private void Start() {
        panel.SetActive(true);

        if(Instance == null){
            Instance = this;
        }else{
            Destroy(this);
        }

        transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }
    // Host a game using Unity Netcode for GameObjects
    public void HostGame()
    {
		if(Steamworks.SteamClient.IsValid){
        	Message_System.LocalMessage("Preparing to launch server...");
        	StartHost(CONSTANTS.MAX_PLAYERS);
		}else{
			Message_System.LocalMessage("Steamworks not initialized!");
		}
    }

    // Join a game using Unity Netcode for GameObjects
    public async void JoinGame()
    {
		if(Steamworks.SteamClient.IsValid){
			await RefreshLobbies();
			if(Lobbies.Count > 0){
				Message_System.LocalMessage("Joining lobby...");
				StartClient(Lobbies[0].Owner.Id);
			}else{
				Message_System.LocalMessage("No lobbies found!");
			}
		}else{
			Message_System.LocalMessage("Steamworks not initialized!");
		}
    }
    private void OnDestroy()
	{
		SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
		SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
		SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
		SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
		SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

		if (NetworkManager.Singleton == null)
			return;

		NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
		NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
	}

	private void OnApplicationQuit() => Disconnect();

	public async void StartHost(uint maxMembers)
	{
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;

		NetworkManager.Singleton.StartHost();

		CurrentLobby = await SteamMatchmaking.CreateLobbyAsync((int)maxMembers);
    }

	public void StartClient(SteamId id)
	{
		NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;

		transport.targetSteamId = id;

		Debug.Log($"Joining room hosted by {transport.targetSteamId}", this);

		if (NetworkManager.Singleton.StartClient())
			Debug.Log("Client has joined!", this);
	}

	public void Disconnect()
	{
		CurrentLobby?.Leave();

		if (NetworkManager.Singleton == null)
			return;

		NetworkManager.Singleton.Shutdown();
	}

	public async Task<bool> RefreshLobbies(int maxResults = 20)
	{
		try
		{
			Lobbies.Clear();

		var lobbies = await SteamMatchmaking.LobbyList
                .FilterDistanceClose()
		.WithMaxResults(maxResults)
		.RequestAsync();

		if (lobbies != null)
		{
			for (int i = 0; i < lobbies.Length; i++)
				Lobbies.Add(lobbies[i]);
		}

		return true;
		}
		catch (System.Exception ex)
		{
			Debug.Log("Error fetching lobbies", this);
			Debug.LogException(ex, this);
			return false;
		}
	}

	private Steamworks.ServerList.Internet GetInternetRequest()
	{
		var request = new Steamworks.ServerList.Internet();
		//request.AddFilter("secure", "1");
		//request.AddFilter("and", "1");
		//request.AddFilter("gametype", "1");
		return request;
	}

    #region Steam Callbacks

	private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
	{
		bool isSame = lobby.Owner.Id.Equals(id);

		Debug.Log($"Owner: {lobby.Owner}");
		Debug.Log($"Id: {id}");
		Debug.Log($"IsSame: {isSame}", this);

		StartClient(id);
	}

	private void OnLobbyInvite(Friend friend, Lobby lobby) => Message_System.LocalMessage($"You got a invite from {friend.Name}");

	private void OnLobbyMemberLeave(Lobby lobby, Friend friend) { }

	private void OnLobbyMemberJoined(Lobby lobby, Friend friend) { }

	private void OnLobbyEntered(Lobby lobby)
    {
		Message_System.LocalMessage($"You have entered in lobby, clientId={NetworkManager.Singleton.LocalClientId}");

		if (NetworkManager.Singleton.IsHost)
			return;

		StartClient(lobby.Owner.Id);
	}

    private void OnLobbyCreated(Result result, Lobby lobby)
	{
		if (result != Result.OK)
        {
			Message_System.LocalMessage($"Lobby couldn't be created!, {result}");
			return;
		}

		lobby.SetFriendsOnly(); // Set to friends only!
		lobby.SetData("name", "Random Cool Lobby");
		lobby.SetJoinable(true);

		Message_System.LocalMessage("Lobby has been created!");
	}

	#endregion

	#region Network Callbacks

	private void ClientConnected(ulong clientId) => Debug.Log($"I'm connected, clientId={clientId}");

    private void ClientDisconnected(ulong clientId)
	{
		Debug.Log($"I'm disconnected, clientId={clientId}");

		NetworkManager.Singleton.OnClientDisconnectCallback -= ClientDisconnected;
		NetworkManager.Singleton.OnClientConnectedCallback -= ClientConnected;
	}

	private void OnServerStarted() {
        Message_System.LocalMessage("Server started!");
        draftManager.enabled = true;
        gameObject.SetActive(false);
     }

    private void OnClientConnectedCallback(ulong clientId) => Debug.Log($"Client connected, clientId={clientId}", this);

    private void OnClientDisconnectCallback(ulong clientId) => Debug.Log($"Client disconnected, clientId={clientId}", this);

    #endregion
}
