using UnityEngine;

public class Trashcan : Interactable
{
    public override void Interact(Player player)
    {
        SoundManager.instance.PlaySound("Trashcan");
        player.ConsumeCurrentItem();
    }

    public override float GetInteractionTime() => 0;

    public override bool CanInteract(Player player) => player.IsHolding && LevelManager.InGame;
}
