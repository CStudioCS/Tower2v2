using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine;

public class SettingsSign: Sign
{
    // Player input persists
    private PlayerInput interactingInput = null;

    public override void Interact(Player player)
	{
        if (interactingInput != null)
			return;

        interactingInput = player.InputPoller.LocalPlayerInput;
        player.PlayerBadge.ShowReadyLabel(false);

        StartCoroutine(OpenMenuNextFrame(player.InputPoller.LocalPlayerInput));
    }

    private IEnumerator OpenMenuNextFrame(PlayerInput localInput)
    {
        // Wait till the next frame to open the menu,
        // otherwise the input action map switch will cause the current interaction to be cancelled immediately
        yield return null;

        localInput.SwitchCurrentActionMap("UI");

        SettingsMenu menu = CanvasLinker.Instance.settingsMenu;
        menu.Closed += OnSettingsClosed;
        menu.Open(localInput);
    }

    private void OnSettingsClosed()
	{
		CanvasLinker.Instance.settingsMenu.Closed -= OnSettingsClosed;

        if (interactingInput == null)
			return;

        interactingInput.SwitchCurrentActionMap("Gameplay");
        LobbyManager.Instance?.SetBadgeVisibility(interactingInput, true);

        interactingInput = null;
    }
}
