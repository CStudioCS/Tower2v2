using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [HideInInspector] public Interactable insideInteractable;
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
    private InputAction cutWoodAction;

    private void Awake()
    {
        interactAction = playerInput.actions.FindAction("Gameplay/Interact");
        cutWoodAction = playerInput.actions.FindAction("Gameplay/CutWood");
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
            StartInteracting(Interactable.InteractionType.Primary, interactAction);
        else if (cutWoodAction.WasPressedThisFrame())
            StartInteracting(Interactable.InteractionType.Secondary, cutWoodAction);
    }

    private void LobbyUpdate()
    {
        if (interactAction.WasPressedThisFrame())
            playerControlBadge.Interact();
    }

    private void StartInteracting(Interactable.InteractionType interactionType, InputAction inputAction)
    {
        // We check whether we're inside an interactable object and if yes if we can interact with it
        if (insideInteractable == null)
            return;
        
        // I do not set isAlreadyInteractedWith to true in an else statement because it only applies to interactables with interaction time // Si qqun comprend ce commentaire de Pierre qu'il se manifeste
        if (insideInteractable.IsAlreadyInteractedWith)
            return;

        if (!insideInteractable.CanInteract(interactionType, this))
            return;

        float time = insideInteractable.GetInteractionTime(interactionType);
        
        // If we can interact instantly, we do it, else we need to wait for the interaction time
        if (time > 0)
            StartCoroutine(InteractTimer(interactionType, time, inputAction));
        else
            insideInteractable.Interact(interactionType, this);
    }

    private IEnumerator InteractTimer(Interactable.InteractionType interactionType, float time, InputAction inputAction)
    {
        Interacting = true;
        insideInteractable.IsAlreadyInteractedWith = true;
        progressBar.StartProgress();
        float t = 0;
        
        while(t < time)
        {
            // If at any point the player stops holding the interact button -> stop interacting
            if (!inputAction.IsPressed())
            {
                StopInteracting();
                yield break;
            }

            progressBar.UpdateProgress(t / time);
            t += Time.deltaTime;
            yield return null;
        }

        // We interacted with the object -> Reset everything and call the interact function
        StopInteracting();
        insideInteractable.Interact(interactionType, this);
    }

    private void StopInteracting()
    {
        Interacting = false;
        insideInteractable.IsAlreadyInteractedWith = false;
        progressBar.ResetProgress();
    }

    // Discards the item currently held
    public void ConsumeCurrentItem()
    {
        IsHolding = false;
        if (HeldItem != null)
            Destroy(HeldItem.gameObject);
        HeldItem = null;
    }

    // Drops to the ground the item currently held
    public void DropItem()
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
}
