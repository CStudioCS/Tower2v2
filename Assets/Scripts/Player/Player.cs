using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public List<Interactable> insideInteractableList { get; private set; }
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

        if (closestInteractable != null)
        {
            if (currentInteractable && currentInteractable != closestInteractable)
                currentInteractable.Highlight(false);

            currentInteractable = closestInteractable;
            currentInteractable.Highlight(true);

            StartInteracting(currentInteractable, interactionType, interactAction);
        }
        else if (HeldItem != null)
            DropHeldItem();
    }

    private void LobbyUpdate()
    {
        if (interactAction.WasPressedThisFrame())
            playerControlBadge.Interact();
    }

    private void StartInteracting(Interactable insideInteractable, Interactable.InteractionType interactionType, InputAction inputAction)
    {
        // We check whether we're inside an interactable object and if yes if we can interact with it
        if (!insideInteractable)
            return;

        Debug.Log($"[StartInteracting] Trying to interact with {insideInteractable.name} with interaction type {interactionType}");

        // I do not set isAlreadyInteractedWith to true in an else statement because it only applies to interactables with interaction time // Si qqun comprend ce commentaire de Pierre qu'il se manifeste
        if (insideInteractable.IsAlreadyInteractedWith)
            return;

        Debug.Log($"[StartInteracting] CanInteract: {insideInteractable.CanInteract(interactionType, this)}, InteractionTime: {insideInteractable.GetInteractionTime(interactionType)}");

        if (!insideInteractable.CanInteract(interactionType, this))
            return;

        Debug.Log($"[StartInteracting] Starting interaction with {insideInteractable.name} with interaction type {interactionType}");

        float time = insideInteractable.GetInteractionTime(interactionType);
        
        // If we can interact instantly, we do it, else we need to wait for the interaction time
        if (time > 0)
            StartCoroutine(InteractTimer(insideInteractable, interactionType, time, inputAction));
        else
            insideInteractable.Interact(interactionType, this);
    }

    private IEnumerator InteractTimer(Interactable insideInteractable, Interactable.InteractionType interactionType, float time, InputAction inputAction)
    {
        Interacting = true;
        insideInteractable.IsAlreadyInteractedWith = true;
        progressBar.StartProgress();
        float t = 0;
        
        while(t < time)
        {
            // If at any point the player stops holding the interact button, or we're not in the game state anymore -> stop interacting
            if (!inputAction.IsPressed() || LevelManager.Instance.GameState != LevelManager.State.Game)
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

    public void GrabNewItem(Item itemPrefab) => GrabItem(Instantiate(itemPrefab, transform));
    
    public void GrabItem(Item item)
    {
        IsHolding = true;
        HeldItem = item;
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
}
