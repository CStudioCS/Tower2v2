using UnityEngine;

public class Item : Interactable
{
    public enum Type { Straw, WoodLog, WoodPlank, Clay, Brick }
    
    private Collider2D itemCollider;
    public Type itemType;

    private void Awake()
    {
        itemCollider = GetComponent<Collider2D>();
        itemCollider.enabled = false;
    }

    protected override bool CanInteractPrimary(Player player) => !player.IsHolding;
    protected override void InteractPrimary(Player player)
    {
        itemCollider.enabled = false;
        player.GrabItem(this);
        transform.SetParent(player.transform);
        transform.position = player.transform.position;
    }

    public void Drop()
    {
        transform.SetParent(null);
        itemCollider.enabled = true;
    }
}
