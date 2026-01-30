using UnityEngine;

public class Item : Interactable
{
    private Collider2D collider2d;
    [SerializeField] private Player.HeldItem givenItem;

    private void Awake()
    {
        collider2d = GetComponent<Collider2D>();
        collider2d.enabled = false;
    }

    public override bool CanInteract(Player player)
        => player.heldItem == Player.HeldItem.Nothing;

    public override void Interact(Player player)
    {
        collider2d.enabled = false;
        player.heldItemGameobject = this.gameObject;
        player.heldItem = givenItem;
        this.transform.SetParent(player.transform);
        this.transform.position = player.transform.position;
        
    }

    public void Drop(Player player)
    {
        player.heldItem = Player.HeldItem.Nothing;
        this.gameObject.transform.SetParent(null);
        collider2d.enabled = true;

    }

}
