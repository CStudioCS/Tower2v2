public class Trashcan : Interactable
{   
    public override void Interact(Player player)
    {
        player.ConsumeCurrentItem();
    }

    public override bool CanInteract(Player player) => player.IsHolding;
}
