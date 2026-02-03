using UnityEngine;

public class Workbench : Interactable
{
    [SerializeField] private GameObject woodPlankItemPrefab;
    
    private State state;
    private SpriteRenderer spriteRenderer;
    private enum State { Empty, HasWoodLog, HasWoodPlank }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override bool CanInteract(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.itemType == Resources.Type.WoodLog;
            case State.HasWoodLog:
                return false;
            case State.HasWoodPlank:
                return !player.IsHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    public override bool CanInteractA(Player player) => state == State.HasWoodLog && !player.IsHolding;

    public override void Interact(Player player)
    {
        if (state == State.Empty)
        {
            state = State.HasWoodLog;
            player.ConsumeCurrentItem();
            spriteRenderer.color = Color.red;
        }
        else if (state == State.HasWoodPlank)
        {
            state = State.Empty;
            player.GrabNewItem(woodPlankItemPrefab);
            spriteRenderer.color = Color.white;
        }
    }

    public override void InteractA(Player player)
    {
        state = State.HasWoodPlank;
        spriteRenderer.color = Color.blue;
    }
}