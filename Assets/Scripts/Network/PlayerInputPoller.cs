using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Runs only on the local machine owning this character.
/// Reads local Unity Input System values and packages them for Fusion.
/// </summary>
public class PlayerInputPoller : NetworkBehaviour
{
    public PlayerInput LocalPlayerInput { get; private set; }
    [Networked] public int SlotIndex { get; set; }

    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction throwAction;

    public override void Spawned()
    {
        if (HasInputAuthority &&LobbyManager.Instance != null)
            LobbyManager.Instance.LinkPendingProxyToAvatar(Object);
    }

    public void AssignLocalInput(PlayerInput assignedInput)
    {
        LocalPlayerInput = assignedInput;

        LocalPlayerInput.SwitchCurrentActionMap("Gameplay");
        LocalPlayerInput.ActivateInput();

        moveAction = LocalPlayerInput.actions.FindAction("Gameplay/Move");
        interactAction = LocalPlayerInput.actions.FindAction("Gameplay/Interact");
        throwAction = LocalPlayerInput.actions.FindAction("Gameplay/Throw");

        // Initialize the PlayerBadge for this player with the correct control scheme
        if (!Enum.TryParse(LocalPlayerInput.currentControlScheme, out PlayerBadge.ControlSchemes controlScheme) ||
            string.IsNullOrEmpty(LocalPlayerInput.currentControlScheme))
        {
            Debug.LogError("Player control scheme not recognized, make sure it was made with a PlayerBadge.ControlSchemes enum value");
            return;
        }

        PlayerBadge badge = GetComponent<Player>().PlayerBadge;
        if (badge != null)
            badge.Initialize(LocalPlayerInput.playerIndex, controlScheme);

        InputDevice device = LocalPlayerInput.devices.Count > 0 ? LocalPlayerInput.devices[0] : null;
        Debug.Log($"Player Joined! Device: {device.name} | Scheme: {controlScheme}");

        RPC_SetSlotIndex(LocalPlayerInput.playerIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetSlotIndex(int index) => SlotIndex = index;

    public PlayerData GetLocalInputData()
    {
        PlayerData data = new PlayerData();

        if (PauseMenu.instance != null && PauseMenu.instance.IsPaused)
            return data;

        if (moveAction != null) data.Movement = moveAction.ReadValue<Vector2>();
        if (interactAction != null) data.Buttons.Set(PlayerInputButtons.Interact, interactAction.IsPressed());
        if (throwAction != null) data.Buttons.Set(PlayerInputButtons.Throw, throwAction.IsPressed());

        return data;
    }
}