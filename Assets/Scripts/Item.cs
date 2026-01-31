using UnityEngine;

public class Item : Interactable
{
    private Collider2D collider2d;
    public Resources.Type itemType;

    private void Awake()
    {
        collider2d = GetComponent<Collider2D>();
        collider2d.enabled = false;
    }

    public override bool CanInteract(Player player)
        => !player.isHolding;

    public override void Interact(Player player)
    {
        collider2d.enabled = false;
        player.isHolding = true;
        player.heldItem = this;
        this.transform.SetParent(player.transform);
        this.transform.position = player.transform.position;
    }

    public void Drop()
    {
        this.gameObject.transform.SetParent(null);
        collider2d.enabled = true;
    }
}
