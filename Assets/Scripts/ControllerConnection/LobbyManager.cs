using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private PlayerInputManager playerInputManager = null;
    
    public event Action<PlayerInput> PlayerJoined;
    public event Action<PlayerInput> PlayerLeft;
    
    public static LobbyManager Instance;
    
    public void Awake()
    {
        if(Instance != null)
            Destroy(Instance);

        Instance = this;
    }
    
    private void Update()
    {
        HandleKeyboardJoinInput();
    }

    private void HandleKeyboardJoinInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
            keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame || keyboard.qKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer("WASD");
        }

        if (keyboard.yKey.wasPressedThisFrame || keyboard.tKey.wasPressedThisFrame || keyboard.fKey.wasPressedThisFrame ||
            keyboard.gKey.wasPressedThisFrame || keyboard.hKey.wasPressedThisFrame || keyboard.rKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer("TFGH");
        }

        if (keyboard.oKey.wasPressedThisFrame || keyboard.iKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame ||
            keyboard.kKey.wasPressedThisFrame || keyboard.lKey.wasPressedThisFrame || keyboard.uKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer("IJKL");
        }

        if (keyboard.rightCtrlKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame ||
            keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer("ArrowKeys");
        }
    }

    private void JoinKeyboardPlayer(string controlSchemeName)
    {
        foreach (PlayerInput playerInput in PlayerInput.all)
        {
            if (playerInput.currentControlScheme == controlSchemeName)
            {
                return;
            }
        }

        PlayerInput newPlayer = playerInputManager.JoinPlayer(
            controlScheme: controlSchemeName,
            pairWithDevice: Keyboard.current
        );
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput == null) return;
        
        PlayerControlBadge badge = playerInput.GetComponentInChildren<PlayerControlBadge>();
        if (badge != null)
        {
            badge.Initialize(playerInput.playerIndex, playerInput.currentControlScheme);
        }
        
        Debug.Log($"Player Joined! Device: {playerInput.devices[0].name} | Scheme: {playerInput.currentControlScheme}");
        PlayerJoined?.Invoke(playerInput);
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        if (playerInput == null) return;

        Debug.Log($"Player {playerInput.playerIndex} left the lobby.");
        PlayerLeft?.Invoke(playerInput);
    }
}