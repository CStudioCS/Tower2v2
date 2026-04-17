using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine;

public class SettingsSign: Sign
{
    private PlayerInput interactingInput = null;

    public override void Interact(Player player)
	{
        if (interactingInput != null)
			return;

		PlayerInputPoller poller = player.GetComponent<PlayerInputPoller>();
		if (poller == null || poller.LocalPlayerInput == null)
			return;

        interactingInput = poller.LocalPlayerInput;
        player.PlayerBadge.ShowReadyLabel(false);
        player.LockInSettingsMenu();

        StartCoroutine(OpenMenuNextFrame(poller.LocalPlayerInput));
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
        Player interactingPlayer = LobbyManager.Instance.GetAvatarForInput(interactingInput);

        if (interactingPlayer != null)
        {
            interactingPlayer.LockInSettingsMenu(false);

            if (interactingPlayer.PlayerBadge != null)
                interactingPlayer.PlayerBadge.ShowReadyLabel(true);
        }

        interactingInput = null;
    }
}
