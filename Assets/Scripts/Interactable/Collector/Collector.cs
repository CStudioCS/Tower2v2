using UnityEngine;

/// <summary>
/// This is everything from wood collectors, cement collectors and dirt collectors. When interacted with, it makes a player hold the itemPrefab gameobject
/// corresponding to the given Item held item (maybe change it with a dictionnary that can match them up ?)
/// </summary>
public class Collector : Interactable
{
    [Header("Collector")]
    [SerializeField] private Item itemPrefab;

    protected override bool CanInteractPrimary(Player player) => !player.IsHolding;

    protected override void InteractPrimary(Player player)
    {
        player.GrabNewItem(itemPrefab);
    }
}
