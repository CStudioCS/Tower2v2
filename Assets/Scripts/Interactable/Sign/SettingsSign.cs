using UnityEngine.InputSystem;

public class SettingsSign: Interactable
{
	private Player interactingPlayer = null;
	
	public override float GetInteractionTime() => 0;

	public override bool CanInteract(Player player)
	{
		return true;
	}

	public override void Interact(Player player)
	{
		if (interactingPlayer != null)
			return;
		
		interactingPlayer = player;
		player.PlayerControlBadge.SetUnready(true);
		player.PlayerControlBadge.ShowReadyLabel(false);
		player.LockInSettingsMenu();

		PlayerInput playerInput = player.GetComponent<PlayerInput>();
		playerInput.SwitchCurrentActionMap("UI");

		SettingsMenu menu = CanvasLinker.Instance.settingsMenu;
		menu.Closed += OnSettingsClosed;
		menu.Open(playerInput);
	}

	private void OnSettingsClosed()
	{
		CanvasLinker.Instance.settingsMenu.Closed -= OnSettingsClosed;

		if (interactingPlayer == null)
			return;

		PlayerInput playerInput = interactingPlayer.GetComponent<PlayerInput>();
		playerInput.SwitchCurrentActionMap("Gameplay");

		interactingPlayer.LockInSettingsMenu(false);
		interactingPlayer.PlayerControlBadge.ShowReadyLabel(true);
		interactingPlayer = null;
	}
}
