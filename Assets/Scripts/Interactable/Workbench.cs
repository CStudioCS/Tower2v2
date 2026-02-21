using UnityEngine;

public class Workbench : Interactable
{
    
    private State state;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private float putOrPickUpItemInteractionTime = 0f;
    [SerializeField] private float cutWoodInteractionTime = 1f;
    private float currentInteractionTime;

    [Header("Prefab refs")]
    [SerializeField] private Item woodPlankItemPrefab;

    [SerializeField] private AudioSource audioSource;

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
                return player.IsHolding && player.HeldItem.ItemType == Item.Type.WoodLog;
            case State.HasWoodLog:
                return !player.IsHolding;
            case State.HasWoodPlank:
                return !player.IsHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    public override void Interact(Player player)
    {
        switch (state)
        {
            case State.Empty:
                state = State.HasWoodLog;
                player.ConsumeCurrentItem();
                currentInteractionTime = cutWoodInteractionTime;

                spriteRenderer.color = Color.red;
                break;
            case State.HasWoodLog:
                state = State.HasWoodPlank;
                currentInteractionTime = putOrPickUpItemInteractionTime;

                spriteRenderer.color = Color.blue;
                break;
            case State.HasWoodPlank:
                state = State.Empty;
                player.GrabNewItem(woodPlankItemPrefab);

                spriteRenderer.color = Color.white;
                break;
        }
    }

    public override float GetInteractionTime() => currentInteractionTime;
    
    protected override void OnGameEnded()
    {
        base.OnGameEnded();
        state = State.Empty;
        spriteRenderer.color = Color.white;
    }

    private void Update()
    {
        if(IsAlreadyInteractedWith && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (!IsAlreadyInteractedWith && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}