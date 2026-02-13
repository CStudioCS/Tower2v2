using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private PlayerInputManager playerInputManager;
    
    public event Action<PlayerInput> PlayerJoined;
    public event Action<PlayerInput> PlayerLeft;
    
    public static LobbyManager Instance;

#if DEBUG
    public bool DebugMode;
#endif

    public void Awake()
    {
        if(Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    private void Update()
    {
        if (LevelManager.Instance.GameState != LevelManager.State.Lobby)
            return;
        
#if DEBUG
        if (DebugMode)
        {
            JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes.WASD);
            JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes.TFGH);
            JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes.IJKL);
            JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes.ArrowKeys);
        }
#endif

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
            JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes.WASD);
        }

        if (keyboard.yKey.wasPressedThisFrame || keyboard.tKey.wasPressedThisFrame || keyboard.fKey.wasPressedThisFrame ||
            keyboard.gKey.wasPressedThisFrame || keyboard.hKey.wasPressedThisFrame || keyboard.rKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes.TFGH);
        }

        if (keyboard.oKey.wasPressedThisFrame || keyboard.iKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame ||
            keyboard.kKey.wasPressedThisFrame || keyboard.lKey.wasPressedThisFrame || keyboard.uKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes.IJKL);
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame ||
            keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes.ArrowKeys);
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
        if (PlayerInput.all.Count >= playerInputManager.maxPlayerCount)
        {
            //This kept triggering on accident everytime I maximized Unity so I commented it (why ?)
            //Debug.Log("An extra gamepad player tried to connect but player limit has been reached");
            return;
        }

        // Check if THIS specific controller is already assigned to a player
        foreach (var player in PlayerInput.all)
        {
            foreach (var device in player.devices)
            {
                if (device == gamepad)
                    return; 
            }
        }

        // Join using the Gamepad device. 
        // Leave controlScheme null or set to "Gamepad" if you have a specific scheme named that.
        playerInputManager.JoinPlayer(
            playerIndex: -1,
            splitScreenIndex: -1,
            controlScheme: PlayerControlBadge.ControlSchemes.Gamepad.ToString(),
            pairWithDevice: gamepad
        );
    }
    
    private void JoinKeyboardPlayer(PlayerControlBadge.ControlSchemes controlSchemeName)
    {
        // 1. Check if we are already at the player limit
        if (PlayerInput.all.Count >= playerInputManager.maxPlayerCount)
        {
            //This kept triggering on accident everytime I maximized Unity so I commented it (why ?)
            //Debug.Log("An extra keyboard tried to connect but player limit has been reached");
            return;
        }

        // 2. Check if a player is already using this specific scheme
        foreach (PlayerInput playerInput in PlayerInput.all)
        {
            if (playerInput.currentControlScheme == controlSchemeName.ToString())
                return;
        }

        // 3. Manually trigger the join
        // We pass -1 for playerIndex to let Unity assign the next available index (0, 1, 2, etc.)
        playerInputManager.JoinPlayer(
            playerIndex: -1, 
            splitScreenIndex: -1,
            controlScheme: controlSchemeName.ToString(), 
            pairWithDevice: Keyboard.current
        );
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput == null) return;

        InputDevice device = playerInput.devices[0];
        if(!Enum.TryParse(playerInput.currentControlScheme, out PlayerControlBadge.ControlSchemes controlScheme))
        {
            Debug.LogError("Player control scheme not recognized, make sure it was made with a PlayerControlBadge.ControlSchemes enum value");
            return;
        }


        PlayerControlBadge badge = playerInput.GetComponent<Player>().PlayerControlBadge;
        if (badge != null)
            badge.Initialize(playerInput.playerIndex, controlScheme);
        
        Debug.Log($"Player Joined! Device: {device.name} | Scheme: {controlScheme}");
        PlayerJoined?.Invoke(playerInput);
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        if (playerInput == null) return;

        Debug.Log($"Player {playerInput.playerIndex} left the lobby.");
        PlayerLeft?.Invoke(playerInput);
    }
}