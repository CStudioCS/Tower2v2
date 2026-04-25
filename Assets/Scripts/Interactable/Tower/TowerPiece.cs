using UnityEngine;

public class TowerPiece : Interactable
{
    [SerializeField] private float basePieceHeight;
    public float BasePieceHeight => basePieceHeight;
    private Tower tower;
    public Tower Tower => tower;
    public override bool CanInteract(Player player) => tower.CanInteract(player);

    public override void Interact(Player player)
    {
        tower.Interact(player);
    }

    public override float GetInteractionTime() => 0;

    public override void RefreshHighlight() => tower.RefreshHighlight();

    public override bool CheckIfCanBeHighlighted(Player player) => tower.CheckIfCanBeHighlighted(player);

    public void Initialize(Tower tower, int sortingOrder)
    {
        this.tower = tower;
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.sortingOrder = sortingOrder;    
        }
    }
}
