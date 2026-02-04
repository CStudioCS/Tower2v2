using UnityEngine;

public class Item : Interactable
{
    public enum Type { Straw, WoodLog, WoodPlank, Clay, Brick }
    
    [Header("Item")]
    [SerializeField] private Type itemType;
    public Type ItemType => itemType;
    
    [SerializeField] private Collider2D itemCollider;
    
    private void Awake()
    {
        itemCollider.enabled = false;
    }

    protected override bool CanInteractPrimary(Player player) => !player.IsHolding;
    protected override void InteractPrimary(Player player)
    {
        itemCollider.enabled = false;
        player.GrabItem(this);
        transform.SetParent(player.transform);
        transform.localPosition = Vector2.zero;
    }

    public void Drop()
    {
        transform.SetParent(null);
        itemCollider.enabled = true;
    }
}
