using UnityEngine;
using static Interactable;

public class TowerPiece : Interactable
{
    private Tower tower;

    private void Awake()
    {
        tower = transform.parent.GetComponent<Tower>();
    }

    public override bool CanInteract(Player player) => tower.CanInteract(player);

    public override void Interact(Player player) => tower.Interact(player);

    public override float GetInteractionTime() => 0;
}
