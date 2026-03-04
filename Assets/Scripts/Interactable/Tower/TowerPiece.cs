using UnityEngine;
using static Interactable;

public class TowerPiece : Interactable
{
    private Tower tower;
    public override bool CanInteract(Player player) => tower.CanInteract(player);

    public override void Interact(Player player) => tower.Interact(player);

    public override float GetInteractionTime() => 0;

    public override void Highlight(bool highlighted) 
    {
        tower.Highlight(highlighted);
    }

    public void Initialize(Tower tower, int sortingOrder)
    {
        this.tower = tower;
        spriteRenderer.sortingOrder = sortingOrder; 
    }
}
