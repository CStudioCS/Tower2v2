using System;
using LitMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public List<Interactable> insideInteractableList = new();
    public bool IsHolding { get; private set; }
    public Item HeldItem { get; private set; }
    public bool Interacting { get; private set; }
    
    [SerializeField] private float minThrowSpeed = 40f;
    [SerializeField] private float maxThrowSpeed = 70f;
    [SerializeField] private float aimChargeDuration = .5f;
    private float aimSpeedRatioVelocity;
    [SerializeField] private float timeBeforeAimCharge = .15f;
    private float timerBeforeAimCharge;

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
    [SerializeField] private PlayerInitPosition playerInitPosition;
    public PlayerInitPosition PlayerInitPosition => playerInitPosition;

    [SerializeField] private ProgressBar progressBar;

    [SerializeField] private PlayerAnimationController playerAnimationController;  // TODO remove reference, fix bad animation coupling
    [SerializeField] private Transform itemParent;

    public Action GrabbedNewItem;

    private InputAction interactAction;
    private InputAction throwAction;
    private Interactable closestInteractable;

    private MotionHandle grabbingLerp;
    private MotionHandle rotationLerp;
    public float throwSpeedRatio { get; private set; }
    private float ThrowSpeed => throwSpeedRatio * (maxThrowSpeed - minThrowSpeed) + minThrowSpeed;
    public Vector2 ThrowDirection => playerMovement.LastNonZeroInput;
    public Vector2 ThrowVelocity => ThrowSpeed * ThrowDirection;

    public bool LockedInSettingsMenu { get; private set; }
    public event Action LockedInSettingsMenuChanged;

    public event Action StartedAimingLockedIn;
    public event Action StoppedAiming;
    public enum AimingState { NotAiming, StartingToAim, AimingLockedIn }

    public AimingState CurrentAimingState { get; private set; } = AimingState.NotAiming;

    private void Awake()
    {
        interactAction = playerInput.actions.FindAction("Gameplay/Interact");
        throwAction = playerInput.actions.FindAction("Gameplay/Throw");
        aimSpeedRatioVelocity = 1 / aimChargeDuration;
    }

    private void Start()
    {
        LevelManager.Instance.GameEnded += OnGameEnded;
    }

    private void Update()
    {
        if (!Interacting)
            UpdateClosestInteractable();

        if (!LockedInSettingsMenu && !PauseMenu.instance.IsPaused)
            HandleInput();

        playerAnimationController.HasItem(HeldItem != null); // TODO fix bad animation coupling
    }

    private void HandleInput()
    {
        switch (LevelManager.Instance.GameState)
        {
            case LevelManager.State.Game:
                HandleInputGame();
                break;
            case LevelManager.State.Lobby:
                HandleInputLobby();
                break;
        }
    }

    private void HandleInputLobby()
    {
        if (interactAction.WasPressedThisFrame() || throwAction.WasPressedThisFrame())
        {
            if (!TryInteract())
            {
                playerControlBadge.Interact();
            }
        }
    }

    private void HandleInputGame()
    {
        switch (CurrentAimingState)
        {
            case AimingState.NotAiming:
                HandleInputNotAiming();
                break;
            case AimingState.StartingToAim:
                HandleInputStartingToAim();
                break;
            case AimingState.AimingLockedIn:
                HandleInputAimingLockedIn();
                break;
        }
    }

    private void HandleInputNotAiming()
    {
        if (IsHolding)
        {
            if (interactAction.WasPressedThisFrame() || throwAction.WasPressedThisFrame())
            {
                if (TryInteract()) // TODO TODO
                    return;
                CurrentAimingState = AimingState.StartingToAim;
                throwSpeedRatio = 0f;
                timerBeforeAimCharge = 0f;
            }
        }
        else
        {
            if (interactAction.WasPressedThisFrame() || throwAction.WasPressedThisFrame())
                TryInteract();
        }
    }

    private void HandleInputStartingToAim()
    {
        if (throwAction.WasReleasedThisFrame())
        {
            ThrowAndExitAim(ThrowVelocity);
            return;
        }
        
        if (interactAction.WasReleasedThisFrame())
        {
            ThrowAndExitAim();
            return;
        }
        
        timerBeforeAimCharge += Time.deltaTime;
        if (timerBeforeAimCharge >= timeBeforeAimCharge)
        {
            CurrentAimingState = AimingState.AimingLockedIn;
            StartedAimingLockedIn?.Invoke();
        }
    }

    private void ThrowAndExitAim() => ThrowAndExitAim(Vector2.zero);
    private void ThrowAndExitAim(Vector2 throwVelocity)
    {
        throwSpeedRatio = 0f;
        CurrentAimingState = AimingState.NotAiming;
        StoppedAiming?.Invoke();
        TryDropHeldItem(throwVelocity);
    }

    private void HandleInputAimingLockedIn()
    {
        throwSpeedRatio = Mathf.Clamp01(throwSpeedRatio + aimSpeedRatioVelocity * Time.deltaTime);
        if (interactAction.WasReleasedThisFrame() || throwAction.WasReleasedThisFrame())
        {
            ThrowAndExitAim(ThrowVelocity);
        }
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
        if (closestInteractable == null)
            return false;
        
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

        return closestInteractable.CheckIfCanBeHighlighted(this);
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
            if (interactAction.WasReleasedThisFrame() || throwAction.WasReleasedThisFrame() || LevelManager.Instance.GameState != LevelManager.State.Game)
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

    public bool TryDropHeldItem() => TryDropHeldItem(Vector2.zero);
    
    /// <summary>
    /// If holding an item, drops to the ground the item currently held
    /// </summary>
    public bool TryDropHeldItem(Vector2 currentThrowSpeed)
    {
        if (!IsHolding || HeldItem.State != Item.ItemState.Held)
            return false;

        HeldItem.State = Item.ItemState.Transitioning;
        playerAnimationController.Drop(); // TODO fix bad animation coupling

        IsHolding = false;
        grabbingLerp.TryCancel();
        rotationLerp.TryCancel();
        HeldItem.Drop(currentThrowSpeed);
        HeldItem = null;
        return true;
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
        CurrentAimingState = AimingState.NotAiming;
        StoppedAiming?.Invoke();
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
