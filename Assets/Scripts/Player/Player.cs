using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using LitMotion;

public class Player : MonoBehaviour
{
    public List<Interactable> insideInteractableList { get; private set; } = new();
    private Interactable currentInteractable;
    public bool IsHolding { get; private set; }
    public Item HeldItem { get; private set; }
    public bool Interacting { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerTeam playerTeam;
    public PlayerTeam PlayerTeam => playerTeam;
    [SerializeField] private PlayerControlBadge playerControlBadge;
    public PlayerControlBadge PlayerControlBadge => playerControlBadge;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private PlayerMovement playerMovement;
    public PlayerMovement PlayerMovement => playerMovement;

    private InputAction interactAction;
    private InputAction secondaryAction;

    private void Awake()
    {
        interactAction = playerInput.actions.FindAction("Gameplay/Interact");
        secondaryAction = playerInput.actions.FindAction("Gameplay/CutWood");
    }

    private void Update()
    {
        switch (LevelManager.Instance.GameState)
        {
            case LevelManager.State.Game: GameUpdate(); break;
            case LevelManager.State.Lobby: LobbyUpdate(); break;
        }
    }

    private void GameUpdate()
    {
        if (interactAction.WasPressedThisFrame())
            Interact(Interactable.InteractionType.Primary);
        else if (secondaryAction.WasPressedThisFrame())
            Interact(Interactable.InteractionType.Secondary);
    }

    private void Interact(Interactable.InteractionType interactionType)
    {
        Interactable closestInteractable = insideInteractableList.Count > 0 ? GetClosestInteractable() : null;

        if (closestInteractable != null && !closestInteractable.IsAlreadyInteractedWith && closestInteractable.CanInteract(interactionType, this))
        {
            if (currentInteractable && currentInteractable != closestInteractable)
                currentInteractable.Highlight(false);

            currentInteractable = closestInteractable;
            currentInteractable.Highlight(true);

            float time = closestInteractable.GetInteractionTime(interactionType);
            if (time > 0)
                StartCoroutine(InteractTimer(closestInteractable, interactionType, time));
            else
                closestInteractable.Interact(interactionType, this);
        }
        else if (HeldItem != null)
            DropHeldItem();
    }

    private void LobbyUpdate()
    {
        if (interactAction.WasPressedThisFrame())
            playerControlBadge.Interact();
    }

    private IEnumerator InteractTimer(Interactable insideInteractable, Interactable.InteractionType interactionType, float time)
    {
        Interacting = true;
        insideInteractable.IsAlreadyInteractedWith = true;
        progressBar.StartProgress();
        float t = 0;
        
        while(t < time)
        {
            // If at any point the player stops holding the interact button, or we're not in the game state anymore -> stop interacting
            if (!GetAssociatedInputAction(interactionType).IsPressed() || LevelManager.Instance.GameState != LevelManager.State.Game)
            {
                StopInteracting(insideInteractable);
                yield break;
            }

            progressBar.UpdateProgress(t / time);
            t += Time.deltaTime;
            yield return null;
        }

        // We interacted with the object -> Reset everything and call the interact function
        StopInteracting(insideInteractable);
        insideInteractable.Interact(interactionType, this);
    }

    private void StopInteracting(Interactable insideInteractable)
    {
        Interacting = false;
        insideInteractable.IsAlreadyInteractedWith = false;
        progressBar.ResetProgress();
    }

    /// <summary>
    /// Discards the item currently held
    /// </summary>
    public void ConsumeCurrentItem()
    {
        IsHolding = false;
        if (HeldItem != null)
            Destroy(HeldItem.gameObject);
        HeldItem = null;
    }

    /// <summary>
    /// Drops to the ground the item currently held
    /// </summary>
    public void DropHeldItem()
    {
        if (!IsHolding)
            return;

        IsHolding = false;
        HeldItem.Drop();
        HeldItem = null;
    }

    public void GrabNewItem(Item itemPrefab)
    {
        Item itemInstance = Instantiate(itemPrefab);
        GrabItem(itemInstance, false);        
    }

    public void GrabItem(Item item, bool interpolatePosition)
    {
        IsHolding = true;
        HeldItem = item;

        item.Immobilize();
        item.LastOwner = this;
        item.transform.SetParent(transform);
        
        if(interpolatePosition)
            LMotion.Create(item.transform.localPosition, Vector3.zero, item.GrabbingTime)
       .BindToLocalPosition(item.transform);
        else
            item.transform.localPosition = Vector2.zero;
    }
    
    private void Start()
    {
        LevelManager.Instance.GameEnded += OnGameEnded;
    }

    private void OnGameEnded()
    {
        Interacting = false;
        ConsumeCurrentItem();
    }
    
    private void OnDisable()
    {
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }

    private Interactable GetClosestInteractable()
    {
        Interactable closest = null;
        float minSqrDistance = float.MaxValue;

        foreach (Interactable interactable in insideInteractableList)
        {
            float sqrDistance = (interactable.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < minSqrDistance)
            {
                minSqrDistance = sqrDistance;
                closest = interactable;
            }
        }

        return closest;
    }

    private InputAction GetAssociatedInputAction(Interactable.InteractionType interactionType)
    {
        switch (interactionType)
        {
            case Interactable.InteractionType.Primary:
                return interactAction;
            case Interactable.InteractionType.Secondary:
                return secondaryAction;
        }

        Debug.LogError("Associated input action was not found");
        return null;
    }
}
