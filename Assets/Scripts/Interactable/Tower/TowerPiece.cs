public class TowerPiece : Interactable
{
    private Tower tower;
    public override bool CanInteract(Player player) => tower.CanInteract(player);

    public override void Interact(Player player)
    {
        tower.Interact(player);
        player.DropHeldItem();
    }

    public override float GetInteractionTime() => 0;

    public override void TryHighlight(bool highlighted, Player player) 
    {
        tower.TryHighlight(highlighted, player);
    }

    public override bool CheckIfCanBeHighlighted(Player player) => tower.CheckIfCanBeHighlighted(player);

    public void Initialize(Tower tower, int sortingOrder)
    {
        this.tower = tower;
        spriteRenderer.sortingOrder = sortingOrder; 
    }
}
