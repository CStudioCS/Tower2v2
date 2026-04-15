using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyManager : NetworkBehaviour, IPlayerLeft
{
    [Header("Setup")]
    [SerializeField] private GameObject localInputProxyPrefab;

    [Header("Network")]
    [SerializeField] private NetworkPrefabRef playerAvatarPrefab;

    // Queue to link the PlayerInput to the avatar.
    public static Queue<PlayerInput> UnlinkedLocalInputs = new Queue<PlayerInput>();
    public static HashSet<string> ActiveKeyboardSchemes = new HashSet<string>();
    public static HashSet<InputDevice> ActiveDevices = new HashSet<InputDevice>();

    private Dictionary<PlayerInput, NetworkObject> activeAvatars = new Dictionary<PlayerInput, NetworkObject>();
    private Dictionary<PlayerRef, int> machinePlayerCounts = new Dictionary<PlayerRef, int>();

    private PlayerInputManager playerInputManager;

    [Networked, OnChangedRender(nameof(OnTotalPlayersChanged))]
    public int TotalPlayers { get; set; }

    public static LobbyManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        playerInputManager = PlayerInputManager.instance;

        ActiveKeyboardSchemes.Clear();
        ActiveDevices.Clear();
        UnlinkedLocalInputs.Clear();
        activeAvatars.Clear();

        foreach (PlayerInput pi in PlayerInput.all)
        {
            if (pi.devices.Count > 0)
            {
                ActiveDevices.Add(pi.devices[0]);
                if (pi.devices[0] is Keyboard)
                    ActiveKeyboardSchemes.Add(pi.currentControlScheme);
            }
        }

        if (playerInputManager != null && localInputProxyPrefab != null)
            playerInputManager.playerPrefab = localInputProxyPrefab;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public override void Spawned()
    {
        machinePlayerCounts.Clear();
        TotalPlayers = 0;

        // Restore the avatars of the players who were in the lobby before the reload
        List<PlayerInput> connectedInputs = new List<PlayerInput>(PlayerInput.all);
        activeAvatars.Clear();
        UnlinkedLocalInputs.Clear();

        // Put them back in the queue so that they can be linked to the new avatars when they spawn
        foreach (PlayerInput input in connectedInputs)
        {
            if (!UnlinkedLocalInputs.Contains(input))
                UnlinkedLocalInputs.Enqueue(input);
        }

        PlayerInput[] pendingArray = UnlinkedLocalInputs.ToArray();

        foreach (PlayerInput input in pendingArray)
        {
            Vector2 targetPosition = default;

            if (NetworkManager.Instance.UseSavedPositionsForNextSpawn && NetworkManager.Instance.SavedPositions.TryGetValue(input, out Vector3 savedPos))
                targetPosition = savedPos;

            RequestSpawnAvatar(targetPosition);
        }

        NetworkManager.Instance.UseSavedPositionsForNextSpawn = false;
        UpdateLocalPlayerCount();
    }

    private void Update()
    {
        if (LevelManager.Instance.GameState != LevelManager.State.Lobby)
            return;

        HandleKeyboardJoinInput();
        HandleGamepadJoinInput();
    }

    private void HandleKeyboardJoinInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
            keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer(PlayerBadge.ControlSchemes.WASD);
        }

        if (keyboard.yKey.wasPressedThisFrame || keyboard.tKey.wasPressedThisFrame || keyboard.fKey.wasPressedThisFrame ||
            keyboard.gKey.wasPressedThisFrame || keyboard.hKey.wasPressedThisFrame || keyboard.rKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer(PlayerBadge.ControlSchemes.TFGH);
        }

        if (keyboard.oKey.wasPressedThisFrame || keyboard.iKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame ||
            keyboard.kKey.wasPressedThisFrame || keyboard.lKey.wasPressedThisFrame || keyboard.uKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer(PlayerBadge.ControlSchemes.IJKL);
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame ||
            keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer(PlayerBadge.ControlSchemes.ArrowKeys);
        }
    }

    private void HandleGamepadJoinInput()
    {
        // Loop through all currently connected gamepads
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame || 
                gamepad.buttonEast.wasPressedThisFrame  || 
                gamepad.buttonWest.wasPressedThisFrame  || 
                gamepad.buttonNorth.wasPressedThisFrame ||
                gamepad.leftShoulder.wasPressedThisFrame || 
                gamepad.rightShoulder.wasPressedThisFrame ||
                gamepad.startButton.wasPressedThisFrame || 
                gamepad.selectButton.wasPressedThisFrame ||
                gamepad.dpad.up.wasPressedThisFrame || 
                gamepad.dpad.down.wasPressedThisFrame ||
                gamepad.dpad.left.wasPressedThisFrame || 
                gamepad.dpad.right.wasPressedThisFrame ||
                gamepad.leftStick.ReadValue().magnitude > 0.1f || 
                gamepad.rightStick.ReadValue().magnitude > 0.1f)
            {
                JoinGamepadPlayer(gamepad);
            }
        }
    }
    
    private void JoinGamepadPlayer(Gamepad gamepad)
    {
        if (ActiveDevices.Contains(gamepad))
            return;

        if (PlayerInput.all.Count >= playerInputManager.maxPlayerCount)
            return;

        ActiveDevices.Add(gamepad);

        // Check if THIS specific controller is already assigned to a player
        foreach (var player in PlayerInput.all)
        {
            foreach (var device in player.devices)
            {
                if (device == gamepad)
                    return; 
            }
        }

        string manufacturer = gamepad.device.description.manufacturer.ToLower();
        string deviceName = gamepad.displayName.ToLower();
        string description = gamepad.description.product.ToLower();

        // Join using the Gamepad device. 
        // Leave controlScheme null or set to "Gamepad" if you have a specific scheme named that.
        if (IsSwitchController(deviceName, description, manufacturer))
        {
            playerInputManager.JoinPlayer(
            playerIndex: -1,
            splitScreenIndex: -1,
            controlScheme: (PlayerBadge.ControlSchemes.Switch).ToString(),
            pairWithDevice: gamepad
            );
        }
        else if (IsPlayStationController(deviceName, description, manufacturer))
        {
            playerInputManager.JoinPlayer(
            playerIndex: -1,
            splitScreenIndex: -1,
            controlScheme: (PlayerBadge.ControlSchemes.PlayStation).ToString(),
            pairWithDevice: gamepad
            );
        }
        else
        {
            playerInputManager.JoinPlayer(
            playerIndex: -1,
            splitScreenIndex: -1,
            controlScheme: PlayerBadge.ControlSchemes.Xbox.ToString(),
            pairWithDevice: gamepad
            );
        }
    }
    bool IsSwitchController(string name, string product, string manufacturer)
    {
        return name.Contains("switch") ||
               name.Contains("joy-con") ||
               product.Contains("switch") ||
               product.Contains("joy-con") ||
               product.Contains("pro controller") ||
               manufacturer.Contains("nintendo");
    }
    bool IsPlayStationController(string name, string product, string manufacturer)
    {
        return name.Contains("dualshock") ||
               name.Contains("dualsense") ||
               name.Contains("wireless controller") ||
               product.Contains("dualshock") ||
               product.Contains("dualsense") ||
               manufacturer.Contains("sony");
    }

    private PlayerInput JoinKeyboardPlayer(PlayerBadge.ControlSchemes controlSchemeName)
    {
        string scheme  = controlSchemeName.ToString();

        if (ActiveKeyboardSchemes.Contains(scheme))
            return null;

        // 1. Check if we are already at the player limit
        if (PlayerInput.all.Count >= playerInputManager.maxPlayerCount)
            return null;

        ActiveKeyboardSchemes.Add(scheme);

        // 2. Check if a player is already using this specific scheme
        foreach (PlayerInput playerInput in PlayerInput.all)
        {
            if (playerInput.currentControlScheme == controlSchemeName.ToString())
                return null;
        }

        // 3. Manually trigger the join
        // We pass -1 for playerIndex to let Unity assign the next available index (0, 1, 2, etc.)
        return playerInputManager.JoinPlayer(
            playerIndex: -1, 
            splitScreenIndex: -1,
            controlScheme: controlSchemeName.ToString(), 
            pairWithDevice: Keyboard.current
        );
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput == null) return;

        // Enqueue the PlayerInput so that the PlayerInputPoller can link it to the avatar when it spawns.
        UnlinkedLocalInputs.Enqueue(playerInput);

        if (Runner != null && Runner.IsRunning)
            RequestSpawnAvatar();
    }

    //<summary>
    // Called by the new Avatar to claim its local proxy
    //</summary>
    public void LinkPendingProxyToAvatar(NetworkObject newAvatar)
    {
        if (UnlinkedLocalInputs.Count > 0)
        {
            PlayerInput pendingInput = UnlinkedLocalInputs.Dequeue();

            string previousActionMap = pendingInput.currentActionMap?.name;

            activeAvatars.Add(pendingInput, newAvatar);
            UpdateLocalPlayerCount();

            PlayerInputPoller poller = newAvatar.GetComponent<PlayerInputPoller>();
            if (poller == null)
                return;

            poller.AssignLocalInput(pendingInput);

            if (previousActionMap == "UI")
            {
                pendingInput.SwitchCurrentActionMap("UI");

                Player player = newAvatar.GetComponent<Player>();
                player.PlayerBadge.ShowReadyLabel(false);
                player.LockInSettingsMenu();
            }
        }
    }

    private void RequestSpawnAvatar(Vector2 spawnPosition = default)
    {
        if (UnlinkedLocalInputs.Count == 0) return;
        
        if (spawnPosition == Vector2.zero)
            spawnPosition = CalculateSpawnPosition();
        

        if (Runner.IsServer)
            Runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, Runner.LocalPlayer);

        else
            RPC_RequestSpawnAvatar(Runner.LocalPlayer, spawnPosition);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestSpawnAvatar(PlayerRef playerRef, Vector2 spawnPosition)
    {
        // Only the Server executes this.
        Runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, playerRef);
    }

    private Vector2 CalculateSpawnPosition()
    {
        StartPoint[] startPoints = StartPointLinker.Instance.startPoints;
        int startPointCount = startPoints.Length;
        bool[] startPointOccupied = new bool[startPointCount];

        List<Player> players = GameStartManager.Instance.Players;

        foreach (Player player in players)
        {
            float minDistance = Mathf.Infinity;
            int closestStartPointIndex = 0;

            for (int i = 0; i < startPointCount; i++)
            {
                float distance = Vector2.SqrMagnitude(startPoints[i].transform.position - player.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestStartPointIndex = i;
                }
            }

            startPointOccupied[closestStartPointIndex] = true;
        }

        PlayerTeam.Team preferredTeam = GameStartManager.Instance.PlayerBalance >= 0 ? PlayerTeam.Team.Left : PlayerTeam.Team.Right;
        StartPoint bestStartPoint = startPoints[0];

        for (int i = 0; i < startPointCount; i++)
        {
            if (!startPointOccupied[i])
            {
                if (startPoints[i].Team == preferredTeam)
                {
                    return startPoints[i].transform.position;
                }
                bestStartPoint = startPoints[i];
            }
        }  
        
        return bestStartPoint.transform.position;
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        if (playerInput == null || playerInput.devices.Count == 0) return;

        InputDevice primaryDevice = playerInput.devices[0];

        if (primaryDevice is Gamepad gamepad)
            ActiveDevices.Remove(gamepad);
        
        else if (primaryDevice is Keyboard && !string.IsNullOrEmpty(playerInput.currentControlScheme))
            ActiveKeyboardSchemes.Remove(playerInput.currentControlScheme);

        if (activeAvatars.TryGetValue(playerInput, out NetworkObject avatarToDestroy))
        {
            if (Runner != null && Runner.IsRunning)
                Runner.Despawn(avatarToDestroy);

            activeAvatars.Remove(playerInput);

            UpdateLocalPlayerCount();
        }

        Debug.Log($"Player {playerInput.playerIndex} left the lobby.");
    }

    public void RespawnLocalPlayers(bool useSavedPositions)
    {
        if (Runner == null || !Runner.IsRunning) return;

        List<PlayerInput> connectedInputs = new List<PlayerInput>(activeAvatars.Keys);
        activeAvatars.Clear();

        foreach (PlayerInput input in connectedInputs)
        {
            Vector3 targetPosition;

            if (useSavedPositions && NetworkManager.Instance.SavedPositions.TryGetValue(input, out Vector3 savedPos))
                targetPosition = savedPos;
            else
                targetPosition = CalculateSpawnPosition();

            // Enqueue the input again for linking after respawn
            if (!UnlinkedLocalInputs.Contains(input))
                UnlinkedLocalInputs.Enqueue(input);

            RequestSpawnAvatar(targetPosition);
        }

        UpdateLocalPlayerCount();
    }

    // <summary>
    // Sending the current count of active avatars on this machine to the server
    // </summary>
    private void UpdateLocalPlayerCount()
    {
        if (Runner == null || !Runner.IsRunning) return;

        RPC_ReportAbsoluteCount(Runner.LocalPlayer, activeAvatars.Count);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_ReportAbsoluteCount(PlayerRef machine, int absoluteCount)
    {
        // Server updates the counts
        machinePlayerCounts[machine] = absoluteCount;
        RecalculateTotal();
    }

    // <summary>
    // This is called when a machine disconnects, it is different from OnPlayerLeft
    // </summary>
    public void PlayerLeft(PlayerRef player)
    {
        if (HasStateAuthority && machinePlayerCounts.ContainsKey(player))
        {
            machinePlayerCounts.Remove(player);
            RecalculateTotal();
        }
    }

    // <summary>
    // This is called on clients when the TotalPlayers property changes
    // </summary>
    void OnTotalPlayersChanged() => NetworkManager.Instance.TriggerPlayersCountChanged();

    private void RecalculateTotal()
    {
        if (!HasStateAuthority) return;

        int newTotal = 0;
        foreach (int count in machinePlayerCounts.Values)
            newTotal += count;

        TotalPlayers = newTotal;

        if (Runner.SessionInfo.IsValid)
        {
            var newProps = new Dictionary<string, SessionProperty>();
            newProps["TotalPlayers"] = TotalPlayers;
            Runner.SessionInfo.UpdateCustomProperties(newProps);
        }
    }

    public Player GetAvatarForInput(PlayerInput input)
    {
        if (activeAvatars.TryGetValue(input, out NetworkObject avatarObj))
            return avatarObj.GetComponent<Player>();

        return null;
    }

    public void SaveCurrentPositions()
    {
        NetworkManager.Instance.SavedPositions.Clear();
        foreach (var kvp in activeAvatars)
        {
            if (kvp.Value != null)
                NetworkManager.Instance.SavedPositions[kvp.Key] = kvp.Value.transform.position;
        }
    }
}