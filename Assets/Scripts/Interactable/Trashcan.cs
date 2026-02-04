public class Trashcan : Interactable
{   
    protected override void InteractPrimary(Player player)
    {
        player.ConsumeCurrentItem();
    }

    protected override bool CanInteractPrimary(Player player) => player.IsHolding;
}
