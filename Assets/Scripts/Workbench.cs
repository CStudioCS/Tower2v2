using UnityEngine;

public class Workbench : Interactable
{
    [SerializeField] private Item woodPlankItemPrefab;
    
    private State state;
    private SpriteRenderer spriteRenderer;
    private enum State { Empty, HasWoodLog, HasWoodPlank }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override bool CanInteractPrimary(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.itemType == WorldResources.Type.WoodLog;
            case State.HasWoodLog:
                return false;
            case State.HasWoodPlank:
                return !player.IsHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    protected override bool CanInteractSecondary(Player player) => state == State.HasWoodLog && !player.IsHolding;

    protected override void InteractPrimary(Player player)
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

    protected override void InteractSecondary(Player player)
    {
        state = State.HasWoodPlank;
        spriteRenderer.color = Color.blue;
    }
}