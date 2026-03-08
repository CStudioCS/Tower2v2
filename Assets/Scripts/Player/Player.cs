using System;
using LitMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public List<Interactable> insideInteractableList { get; private set; } = new();
    public bool IsHolding { get; private set; }
    public Item HeldItem { get; private set; }
    public bool Interacting { get; private set; }

    [Header("References")]

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerTeam playerTeam;
    public PlayerTeam PlayerTeam => playerTeam;
    [SerializeField] private PlayerControlBadge playerControlBadge;
    public PlayerControlBadge PlayerControlBadge => playerControlBadge;
    [SerializeField] private PlayerMovement playerMovement;
    public PlayerMovement PlayerMovement => playerMovement;
    
    [SerializeField] private PlayerStats playerStats;
    public PlayerStats PlayerStats => playerStats;

    [SerializeField] private ProgressBar progressBar;

    [SerializeField] private PlayerAnimationController playerAnimationController;  // TODO remove reference, fix bad animation coupling
    [SerializeField] private Transform itemParent;

    public Action GrabbedNewItem;

    private InputAction interactAction;
    private Interactable closestInteractable;

    private MotionHandle grabbingLerp;
    private MotionHandle rotationLerp;
    
    public bool LockedInSettingsMenu { get; private set; }
    public event Action LockedInSettingsMenuChanged;

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
        if (!Interacting)
            UpdateClosestInteractable();

        if (interactAction.WasPressedThisFrame() && !LockedInSettingsMenu && !PauseMenu.instance.IsPaused)
        {
            bool successfulInteraction = TryInteract();
            if (!successfulInteraction && LevelManager.Instance.GameState == LevelManager.State.Lobby)
            {
                playerControlBadge.Interact();
            }
        }

        playerAnimationController.HasItem(HeldItem != null); // TODO fix bad animation coupling
    }

    private void UpdateClosestInteractable()
    {
        Interactable newClosestInteractable = insideInteractableList.Count > 0 ? GetClosestInteractable() : null;

        if (closestInteractable != newClosestInteractable)
        {
            if (closestInteractable != null)
                closestInteractable.TryHighlight(false, this);
            
            if (newClosestInteractable != null)
                newClosestInteractable.TryHighlight(true, this);
        }

        closestInteractable = newClosestInteractable;
    }
    private bool TryInteract()
    {
        if (closestInteractable != null)
        {
            float time = closestInteractable.GetInteractionTime();
            if (time > 0)
            {
                if (closestInteractable is Workbench)
                    playerAnimationController.StartCutting(); // TODO fix bad animation coupling
                else if (closestInteractable is Collector)
                    playerAnimationController.StartCollecting(); // TODO fix bad animation coupling
                else
                    Debug.LogError("This Interactable is not currently supported by the animator");
                    //no interactable in the game takes time aside from Collector and Workbench as of rn
                
                StartCoroutine(InteractTimer(closestInteractable, time));
            }
            else
                closestInteractable.Interact(this);

            return true;
        }
        if (HeldItem != null)
        {
            DropHeldItem();
            return true;
        }

        return false;
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
        playerAnimationController.EndInteraction(); // TODO fix bad animation coupling
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
        if (!IsHolding || (HeldItem.State != Item.ItemState.Held))
            return;

        HeldItem.State = Item.ItemState.Transitioning;
        playerAnimationController.Drop(); // TODO fix bad animation coupling

        IsHolding = false;
        grabbingLerp.TryCancel();
        rotationLerp.TryCancel();
        HeldItem.Drop();
        HeldItem = null;
    }

    /// <summary>
    /// Makes the player grab a newly instantiated Item
    /// </summary>
    /// <param name="itemPrefab"></param>
    /// <param name="originallyCollectedByTeam">The team this item was originally collected by. If left null this will be set as this player's team</param>
    public void GrabNewItem(Item itemPrefab, PlayerTeam.Team? originallyCollectedByTeam = null)
    {
        Item itemInstance = Instantiate(itemPrefab);
        GrabItem(itemInstance, false);
        if (originallyCollectedByTeam is PlayerTeam.Team team)
            itemInstance.originallyCollectedByTeam = team;
        else
            itemInstance.originallyCollectedByTeam = playerTeam.CurrentTeam;

        GrabbedNewItem?.Invoke();
    }

    public void GrabItem(Item item, bool interpolatePosition)
    {
        playerAnimationController.Grab(); // TODO fix bad animation coupling

        IsHolding = true;
        HeldItem = item;

        item.Immobilize();
        item.LastOwner = this;
        item.transform.SetParent(itemParent);
        item.transform.localRotation = Quaternion.identity;

        if (interpolatePosition)
        {
            grabbingLerp = LMotion.Create((Vector2) item.transform.localPosition, Vector2.zero, item.GrabbingTime).Bind(position => { if (item != null) item.transform.localPosition = position; });
            rotationLerp = LMotion.Create(item.transform.localRotation, Quaternion.identity, item.GrabbingTime).Bind(rotation => { if (item != null) item.transform.localRotation = rotation; });
        }
        else
            item.transform.localPosition = Vector2.zero;

        item.State = Item.ItemState.Held;
    }

    private void OnGameEnded()
    {
        Interacting = false;
        ConsumeCurrentItem();

        if (closestInteractable != null)
            closestInteractable.TryHighlight(false, this);
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
            float sqrDistance = ((Vector2)interactable.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance < minSqrDistance && !interactable.IsAlreadyInteractedWith && interactable.CanInteract(this))
            {
                minSqrDistance = sqrDistance;
                closest = interactable;
            }
        }

        return closest;
    }

    public void LockInSettingsMenu(bool locked = true)
    {
        LockedInSettingsMenu = locked;
        LockedInSettingsMenuChanged?.Invoke();
    }
}
