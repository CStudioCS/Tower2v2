using LitMotion;
using LitMotion.Adapters;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField] private PlayerAnimationController playerAnimationController;

    private InputAction interactAction;
    private Interactable closestInteractable;

    private MotionHandle grabbingLerp;
    private MotionHandle rotationLerp;

    private void Awake()
    {
        interactAction = playerInput.actions.FindAction("Gameplay/Interact");
    }

    private void Start()
    {
        LevelManager.Instance.GameEnded += OnGameEnded;
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
        UpdateClosestInteractable();

        if (interactAction.WasPressedThisFrame())
            Interact();

        playerAnimationController.HasItem(HeldItem != null);
    }

    private void UpdateClosestInteractable()
    {
        closestInteractable = insideInteractableList.Count > 0 ? GetClosestInteractable() : null;

        if (closestInteractable != null)
        {
            if (currentInteractable && currentInteractable != closestInteractable)
                currentInteractable.Highlight(false);

            currentInteractable = closestInteractable;

            currentInteractable.Highlight(true);
        }
    }
    private void Interact()
    {
        if (closestInteractable != null)
        {
            float time = closestInteractable.GetInteractionTime();
            if (time > 0)
            {
                if (closestInteractable is Workbench)
                    playerAnimationController.StartCutting();
                else if (closestInteractable is Collector)
                    playerAnimationController.StartCollecting();
                else
                    Debug.LogError("This Interactable is not currently supported by the animator");
                    //no interactable in the game takes time aside from Collector and Workbench as of rn
                
                StartCoroutine(InteractTimer(closestInteractable, time));
            }
            else
                closestInteractable.Interact(this);
        }
        else if (HeldItem != null)
            DropHeldItem();

    }

    private void LobbyUpdate()
    {
        if (interactAction.WasPressedThisFrame())
            playerControlBadge.Interact();
    }

    private IEnumerator InteractTimer(Interactable insideInteractable, float time)
    {
        Interacting = true;
        insideInteractable.IsAlreadyInteractedWith = true;
        progressBar.StartProgress();
        float t = 0;
        
        while(t < time)
        {
            // If at any point the player stops holding the interact button, or we're not in the game state anymore -> stop interacting
            if (!interactAction.IsPressed() || LevelManager.Instance.GameState != LevelManager.State.Game)
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
        insideInteractable.Interact(this);
    }

    private void StopInteracting(Interactable insideInteractable)
    {
        playerAnimationController.EndInteraction();
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

        playerAnimationController.Drop();

        IsHolding = false;
        grabbingLerp.TryCancel();
        rotationLerp.TryCancel();
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
        playerAnimationController.Grab();

        IsHolding = true;
        HeldItem = item;

        item.Immobilize();
        item.LastOwner = this;
        item.transform.SetParent(transform);

        if (interpolatePosition)
        {
            grabbingLerp = LMotion.Create(item.transform.localPosition, Vector3.zero, item.GrabbingTime).Bind(x => item.transform.localPosition = x);
            rotationLerp = LMotion.Create(item.transform.localRotation, Quaternion.identity, item.GrabbingTime).Bind(x => item.transform.localRotation = x);
        }
        else
            item.transform.localPosition = Vector2.zero;
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
            if (sqrDistance < minSqrDistance && !interactable.IsAlreadyInteractedWith && interactable.CanInteract(this))
            {
                minSqrDistance = sqrDistance;
                closest = interactable;
            }
        }

        return closest;
    }
}
