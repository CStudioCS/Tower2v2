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
			player.PlayerControlBadge.SetReadyText(signText);
			player.PlayerControlBadge.SetUnready(true);
		}
		else
			player.PlayerControlBadge.ResetReadyText();
		
		base.TryHighlight(highlighted, player);
	}
}
