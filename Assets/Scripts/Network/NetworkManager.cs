using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Handles the global connection to Photon Fusion.
/// This is the only script allowed to start or shutdown the NetworkRunner.
/// </summary>
public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }

    [HideInInspector] public NetworkRunner Runner;
    public event Action<List<SessionInfo>> OnSessionListUpdatedEvent;

    public bool IsSinglePlayer => Runner != null && Runner.IsRunning && Runner.GameMode == GameMode.Single;
    public bool IsHosting => Runner != null && Runner.IsRunning && Runner.IsServer && !IsSinglePlayer;
    public bool IsClient => Runner != null && Runner.IsRunning && Runner.IsClient && !IsSinglePlayer;
    public bool IsBusy { get; private set; }
    public string CurrentSessionName => (Runner != null && Runner.SessionInfo.IsValid) ? Runner.SessionInfo.Name : "";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
            runnerObj.transform.SetParent(this.transform);
            Runner = runnerObj.AddComponent<NetworkRunner>();
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
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            };

            // Start the connection
            StartGameResult result = await Runner.StartGame(startGameArgs);
            if (!result.Ok) Debug.LogError($"[NetworkManager] Failed hosting a game: {result.ShutdownReason}");
        }
        finally { IsBusy = false; }
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
            runnerObj.transform.SetParent(this.transform);
            Runner = runnerObj.AddComponent<NetworkRunner>();
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

    // --- Required callbacks ---
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
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
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}