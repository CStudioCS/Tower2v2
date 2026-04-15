using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the global connection to Photon Fusion.
/// This is the only script allowed to start or shutdown the NetworkRunner.
/// </summary>
public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Managers")]
    [SerializeField] private NetworkPrefabRef lobbyManagerPrefab;

    public static NetworkManager Instance { get; private set; }

    [HideInInspector] public NetworkRunner Runner;
    public event Action<List<SessionInfo>> OnSessionListUpdatedEvent;
    public static event Action<NetworkRunner, NetworkInput> OnProvideInputEvent;
    public static event Action<GameMode> OnNetworkModeInitialized;
    public static event Action OnPlayersCountChanged;

    public Dictionary<PlayerInput, Vector3> SavedPositions { get; private set; } = new Dictionary<PlayerInput, Vector3>();
    public bool UseSavedPositionsForNextSpawn { get; set; }

    public bool IsSinglePlayer => Runner != null && Runner.IsRunning && Runner.GameMode == GameMode.Single;
    public bool IsHosting => Runner != null && Runner.IsRunning && Runner.IsServer && !IsSinglePlayer;
    public bool IsClient => Runner != null && Runner.IsRunning && Runner.IsClient && !IsSinglePlayer;
    public bool IsBusy { get; private set; }
    public string CurrentSessionName => (Runner != null && Runner.SessionInfo.IsValid) ? Runner.SessionInfo.Name : "";
    public void TriggerPlayersCountChanged() => OnPlayersCountChanged?.Invoke();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private async void Start()
    {
        _ = StartNetworkGame(GameMode.Single);
    }

    /// <summary>
    /// Starts the game as a Host or Client.
    /// </summary>
    public async Task StartNetworkGame(GameMode mode, string roomName = "")
    {
        IsBusy = true; // Lock the network state
        try
        {
            // Clean up any existing runner (online or offline)
            if (Runner != null) await InternalDisconnect();

            GameObject runnerObj = new GameObject("NetworkRunner");
            Runner = runnerObj.AddComponent<NetworkRunner>();
            runnerObj.AddComponent<Fusion.Addons.Physics.RunnerSimulatePhysics2D>();

            // Tell the Runner attached to the child gameeobject that this script will handle its callbacks
            Runner.AddCallbacks(this);

            Runner.ProvideInput = true; // Tells Fusion this client will provide inputs
            var customProps = new Dictionary<string, SessionProperty>();
            customProps["TotalPlayers"] = 1; // Host will update this later.

            // Configure the connection
            var startGameArgs = new StartGameArgs()
            {
                GameMode = mode,
                SessionName = (mode == GameMode.Single) ? "" : roomName,
                SessionProperties = customProps,
                SceneManager = runnerObj.AddComponent<NetworkSceneManagerDefault>(),
                Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
            };

            // Start the connection
            StartGameResult result = await Runner.StartGame(startGameArgs);
            if (!result.Ok) 
                Debug.LogError($"[NetworkManager] Failed hosting a game: {result.ShutdownReason}");
            else
                OnNetworkModeInitialized?.Invoke(mode);
        }
        finally { IsBusy = false; }
    }

    // This method is an old artefact from the early development
    // I could've abondoned it beacause I implemented a way of switching the runner witout reloading the scene
    // But I kept it and use it only once when switching to offline mode because I think it's more impactful from UX pov
    public async void Reboot(GameMode targetMode)
    {
        if (Runner != null) await InternalDisconnect();

        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

        while (!asyncLoad.isDone)
            await Task.Yield();
            
        _ = StartNetworkGame(targetMode);
    }

    /// <summary>
    /// Connects to the Master Server to browse available sessions (Lobby).
    /// </summary>
    public async Task JoinLobby()
    {
        IsBusy = true;
        try
        {
            // Clean up any existing runner (online or offline)
            if (Runner != null) await InternalDisconnect();

            GameObject runnerObj = new GameObject("NetworkRunner");
            Runner = runnerObj.AddComponent<NetworkRunner>();
            runnerObj.AddComponent<Fusion.Addons.Physics.RunnerSimulatePhysics2D>();


            // Tell the Runner attached to the child gameeobject that this script will handle its callbacks
            Runner.AddCallbacks(this);

            Runner.ProvideInput = true;

            // Tells Fusion to join the lobby and start listening for session updates
            var result = await Runner.JoinSessionLobby(SessionLobby.ClientServer);

            if (!result.Ok)
                Debug.LogError($"[NetworkManager] Failed joining the lobby: {result.ShutdownReason}");
        } 
        finally { IsBusy = false; }
    }

    /// <summary>
    /// Disconnects the player and destroys the Runner properly.
    /// </summary>
    public async Task Disconnect()
    {
        IsBusy = true;
        try { 
            await InternalDisconnect(); 
        }
        finally { 
            IsBusy = false; 
        }
    }

    private async Task InternalDisconnect()
    {
        if (Runner != null)
        {
            Debug.Log("[NetworkManager] Shutting down current Runner...");
            GameObject runnerObj = Runner.gameObject;
            await Runner.Shutdown();

            if (runnerObj != null) Destroy(runnerObj);
            Runner = null;
        }
    }

    // ==============================
    // INetworkRunnerCallbacks 
    // ==============================

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] Network player {player.PlayerId} joined the session!");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Fusion] Network player {player.PlayerId} left the session.");
        if (runner.IsServer)
        {
            // TODO: I will add a cleaning logic
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        OnSessionListUpdatedEvent?.Invoke(sessionList);
        Debug.Log($"[Fusion] Server list updated. {sessionList.Count} lobby(ies) found.");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason info)
    {
        Debug.Log($"[Fusion] Shutdown: {info}");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        PlayerNetworkInput collectiveInput = new PlayerNetworkInput();

        // Horrible but will change promise!
        PlayerInputPoller[] pollers = FindObjectsByType<PlayerInputPoller>(FindObjectsSortMode.None);

        foreach (var poller in pollers)
        {
            if (poller.HasInputAuthority && poller.LocalPlayerInput != null)
            {
                int index = poller.LocalPlayerInput.playerIndex;
                PlayerData data = poller.GetLocalInputData();

                if (index == 0) collectiveInput.Player0 = data;
                else if (index == 1) collectiveInput.Player1 = data;
                else if (index == 2) collectiveInput.Player2 = data;
                else if (index == 3) collectiveInput.Player3 = data;
            }
        }

        input.Set(collectiveInput); // Ship the cargo yahooo!
        OnProvideInputEvent?.Invoke(runner, input);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (runner.IsServer)
            runner.Spawn(lobbyManagerPrefab);
    }

    // --- Required callbacks ---
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}