using UnityEngine;

public class Sign: Interactable
{
	[SerializeField] private string signText;
	
	public override float GetInteractionTime() => 0;

	public override bool CanInteract(Player player) => true;

	public override void OnTargetedBy(Player player, bool targeted)
	{
		if (!player.HasInputAuthority)
			return;

		if (targeted)
		{
			player.PlayerBadge.SetReadyText(signText);
			player.PlayerControlBadge.SetUnready();
		}
		else
			player.PlayerBadge.ResetReadyText();
	}
}