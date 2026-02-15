using UnityEngine;

public class Workbench : Interactable
{
    
    private State state;
    private SpriteRenderer spriteRenderer;

    private float giveOrTakeItemInteractionTime = 0f;
    [SerializeField] private float cutWoodInteractionTime = 1f;

    [Header("Prefab refs")]
    [SerializeField] private Item woodPlankItemPrefab;

    private enum State { Empty, HasWoodLog, HasWoodPlank }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        giveOrTakeItemInteractionTime = interactionTime;
    }

    protected override bool CanInteractPrimary(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.ItemType == Item.Type.WoodLog;
            case State.HasWoodLog:
                return !player.IsHolding;
            case State.HasWoodPlank:
                return !player.IsHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    protected override void InteractPrimary(Player player)
    {
        switch (state)
        {
            case State.Empty:
                state = State.HasWoodLog;
                player.ConsumeCurrentItem();
                interactionTime = cutWoodInteractionTime;

                spriteRenderer.color = Color.red;
                break;
            case State.HasWoodLog:
                state = State.HasWoodPlank;
                interactionTime = giveOrTakeItemInteractionTime;

                spriteRenderer.color = Color.blue;
                break;
            case State.HasWoodPlank:
                state = State.Empty;
                player.GrabNewItem(woodPlankItemPrefab);

                spriteRenderer.color = Color.white;
                break;
        }
    }
    
    protected override void OnGameEnded()
    {
        base.OnGameEnded();
        state = State.Empty;
        spriteRenderer.color = Color.white;
    }
}