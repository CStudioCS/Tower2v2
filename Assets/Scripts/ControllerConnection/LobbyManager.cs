using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private PlayerInputManager playerInputManager = null;

    [SerializeField] private Transform leftTeamContainer;
    [SerializeField] private Transform rightTeamContainer;
    
    
    private void Update()
    {
        HandleKeyboardJoinInput();
    }

    private void HandleKeyboardJoinInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer("WASD");
        }

        if (keyboard.yKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer("TFGH");
        }

        if (keyboard.oKey.wasPressedThisFrame)
        {
            JoinKeyboardPlayer("IJKL");
        }

        if (keyboard.rightCtrlKey.wasPressedThisFrame)
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

        Debug.Log($"Keyboard player joined with scheme: {controlSchemeName}");
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (playerInput == null) return;

        Debug.Log($"Player Joined! Device: {playerInput.devices[0].name} | Scheme: {playerInput.currentControlScheme}");

        PlayerControlBadge badge = playerInput.GetComponent<PlayerControlBadge>();
        if (badge != null)
        {
            badge.Setup(playerInput.playerIndex, playerInput.currentControlScheme, leftTeamContainer, rightTeamContainer);
        }
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        if (playerInput == null) return;

        Debug.Log($"Player {playerInput.playerIndex} left the lobby.");
    }
}