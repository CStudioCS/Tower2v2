using UnityEngine;
using static Interactable;

public class TowerPiece : Interactable
{
    private Tower tower;

    private void Awake()
    {
        tower = transform.parent.GetComponent<Tower>();
    }

    public override bool CanInteract(InteractionType type, Player player)
    {
        return tower.CanInteract(type,player);
    }

    public override void Interact(InteractionType type, Player player)
    {
        tower.Interact(type,player);
    }
}
