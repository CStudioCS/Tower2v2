using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

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

		EventSystem eventSystem = CanvasLinker.Instance.settingsMenu.EventSystem;
		InputSystemUIInputModule inputModule = CanvasLinker.Instance.settingsMenu.InputSystemUIInputModule;
		eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
		PlayerInput playerInput = player.GetComponent<PlayerInput>();
		inputModule.actionsAsset = playerInput.actions;
	}
}
