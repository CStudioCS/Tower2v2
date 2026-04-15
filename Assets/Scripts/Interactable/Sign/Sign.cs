using UnityEngine;

public class Sign: Interactable
{
	[SerializeField] private string signText;
	
	public override float GetInteractionTime() => 0;

	public override bool CanInteract(Player player) => true;
	
	public override void TryHighlight(bool highlighted, Player player)
	{
		if (highlighted)
		{
			player.PlayerBadge.SetReadyText(signText);
			player.PlayerControlBadge.SetUnready();
		}
		else
			player.PlayerBadge.ResetReadyText();
		
		base.TryHighlight(highlighted, player);
	}
}
