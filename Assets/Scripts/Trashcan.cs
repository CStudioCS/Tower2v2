public class Trashcan : Interactable
{   
    public override void Interact(Player player)
    {
        player.isHolding = false;

        if (player.heldItemGameobject != null)
            Destroy(player.heldItemGameobject);

        player.heldItemGameobject = null;
    }

    public override bool CanInteract(Player player)
        => player.isHolding;
}
