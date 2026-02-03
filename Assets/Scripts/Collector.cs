using UnityEngine;

/// <summary>
/// This is everything from wood collectors, cement collectors and dirt collectors. When interacted with, it makes a player hold the itemPrefab gameobject
/// corresponding to the given Item held item (maybe change it with a dictionnary that can match them up ?)
/// </summary>
public class Collector : Interactable
{
    [SerializeField] private Item itemPrefab;

    public override bool CanInteract(Player player) => !player.IsHolding;

    public override void Interact(Player player)
    {
        player.GrabNewItem(itemPrefab);
    }
}
