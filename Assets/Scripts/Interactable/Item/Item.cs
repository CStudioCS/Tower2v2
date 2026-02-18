using UnityEngine;

public class Item : Interactable
{
    public enum Type { Straw, WoodLog, WoodPlank, Clay, Brick }
    
    [Header("Item")]
    [SerializeField] private Type itemType;
    public Type ItemType => itemType;
    private Player lastOwner;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D itemCollider;
    [SerializeField] private float ejectionCoefficient;
    [SerializeField] private float rotationCoefficient;

    private void Awake()
    {
        itemCollider.enabled = false;
        LevelManager.Instance.GameEnded += Disappear;
    }

    public override bool CanInteract(Player player) => !player.IsHolding;

    public override void Interact(Player player)
    {
        Grab(player);
    }

    public override float GetInteractionTime() => 0;

    public void Grab(Player player)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.simulated = false;
        transform.rotation = Quaternion.identity;
        lastOwner = player;
        itemCollider.enabled = false;
        player.GrabItem(this);
        transform.SetParent(player.transform);
        transform.localPosition = Vector2.zero;
    }

    public void Drop()
    {
        Vector2 speedVector = lastOwner.PlayerMovement.Rb.linearVelocity;
        lastOwner = null;
        transform.SetParent(null);
        rb.simulated = true;
        itemCollider.enabled = true;
        rb.linearVelocity = ejectionCoefficient * speedVector ;
        rb.angularVelocity = Random.Range(-1,2) * rotationCoefficient;
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        LevelManager.Instance.GameEnded -= Disappear;
    }
}
