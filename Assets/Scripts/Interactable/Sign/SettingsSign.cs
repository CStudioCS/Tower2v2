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
		CanvasLinker.Instance.settingsMenu.ShowSettings();
	}
}
