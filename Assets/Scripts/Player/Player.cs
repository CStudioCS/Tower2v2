using Fusion;
using LitMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class Player : NetworkBehaviour
{
    public List<Interactable> insideInteractableList { get; } = new();
    public bool IsHolding => HeldItem != null;
    public Item HeldItem { get; private set; }
    public bool Interacting { get; private set; }
    
    [SerializeField] private float minThrowSpeed = 40f;
    [SerializeField] private float maxThrowSpeed = 70f;
    [SerializeField] private float aimChargeDuration = .5f;
    private float aimSpeedRatioVelocity;
    [SerializeField] private float timeBeforeAimCharge = .15f;
    private float timerBeforeAimCharge;

    [Header("References")]

    [SerializeField] private PlayerTeam playerTeam;
    public PlayerTeam PlayerTeam => playerTeam;

    [SerializeField] private PlayerControlBadge playerControlBadge;
    public PlayerControlBadge PlayerControlBadge => playerControlBadge;

    [SerializeField] private PlayerBadge playerBadge;
    public PlayerBadge PlayerBadge => playerBadge;

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

    private Interactable closestInteractable;
    private Interactable currentTargetInteractable;
    private float currentInteractionDuration;

    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked] public TickTimer InteractionTimer { get; set; }

    private PlayerData currentTickData;


    private MotionHandle grabbingLerp;
    private MotionHandle rotationLerp;
    public float throwSpeedRatio { get; private set; }
    private float ThrowSpeed => throwSpeedRatio * (maxThrowSpeed - minThrowSpeed) + minThrowSpeed;
    public Vector2 ThrowDirection => playerMovement.LastNonZeroInput;
    public Vector2 ThrowVelocity => ThrowSpeed * ThrowDirection;

    public bool LockedInSettingsMenu { get; private set; }
    public event Action LockedInSettingsMenuChanged;

    public event Action AvatarSpawned;
    public event Action StartedAimingLockedIn;
    public event Action StoppedAiming;
    public enum AimingState { NotAiming, StartingToAim, AimingLockedIn }

    public AimingState CurrentAimingState { get; private set; } = AimingState.NotAiming;

    private void Awake()
    {
        aimSpeedRatioVelocity = 1 / aimChargeDuration;
    }

    private void Start()
    {
        LevelManager.Instance.GameEndedOrReturnedToLobby += OnGameEndedOrReturnedToLobby;
    }

    public override void Spawned()
    {
        GameStartManager.Instance.AddPlayer(this);
        AvatarSpawned?.Invoke();
    }

    public override void Despawned(NetworkRunner runner, bool hasState) => GameStartManager.Instance.RemovePlayer(this);

    public override void FixedUpdateNetwork()
    {
        if (!Interacting)
            UpdateClosestInteractable();

        if (GetInput(out PlayerNetworkInput input))
        {
            int mySlot = GetComponent<PlayerInputPoller>().SlotIndex;

            PlayerData myData = default;
            if (mySlot == 0) myData = input.Player0;
            else if (mySlot == 1) myData = input.Player1;
            else if (mySlot == 2) myData = input.Player2;
            else if (mySlot == 3) myData = input.Player3;


            currentTickData = myData;

            if (!LockedInSettingsMenu && !PauseMenu.instance.IsPaused)
                HandleInput();

            PreviousButtons = myData.Buttons;
        }

        playerAnimationController.HasItem(HeldItem != null); // TODO fix bad animation coupling
    }

    private void HandleInput()
    {
        switch (LevelManager.Instance.GameState)
        {
            case LevelManager.State.Game:
                if (Interacting)
                    ProcessLongInteraction();
                else
                    HandleInputGame();
                break;
            case LevelManager.State.Lobby:
                HandleInputLobby();
                break;
        }
    }

    private void HandleInputLobby()
    {
        bool interactPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Interact);
        bool throwPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Throw);

        if (interactPressed || throwPressed)
        {
            if (!TryInteract())
                playerControlBadge.ToggleReady();
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
        bool interactPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Interact);
        bool throwPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Throw);

        if (IsHolding)
        {
            if (interactPressed || throwPressed)
            {
                if (TryInteract())
                    return;
                CurrentAimingState = AimingState.StartingToAim;
                throwSpeedRatio = 0f;
                timerBeforeAimCharge = 0f;
            }
        }
        else
        {
            if (interactPressed || throwPressed)
                TryInteract();
        }
    }

    private void HandleInputStartingToAim()
    {
        bool interactPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Interact);
        bool throwPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Throw);

        if (throwPressed)
        {
            ThrowAndExitAim(ThrowVelocity);
            return;
        }

        // If the player releases the interact button BUT is still holding the throw button at the same time,
        // their intention is probably to throw the item, so we should not drop it and stay in the StartingToAim state.
        if (interactPressed && !throwPressed)
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
        bool interactPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Interact);
        bool throwPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Throw);

        throwSpeedRatio = Mathf.Clamp01(throwSpeedRatio + aimSpeedRatioVelocity * Time.deltaTime);
        if (interactPressed || throwPressed)
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

        bool canBeHighlighted = closestInteractable.CheckIfCanBeHighlighted(this);
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

            // ---- Start the interaction ----
            // Before the network update it was a couroutine

            Interacting = true;
            currentTargetInteractable = closestInteractable;
            currentInteractionDuration = time;
            currentTargetInteractable.IsAlreadyInteractedWith = true;

            progressBar.StartProgress();

            InteractionTimer = TickTimer.CreateFromSeconds(Runner, time);
        }
        else
            ExecuteInteraction(closestInteractable);

        return canBeHighlighted;
    }

    private void ProcessLongInteraction()
    {
        bool interactPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Interact);
        bool throwPressed = currentTickData.Buttons.WasPressed(PreviousButtons, PlayerInputButtons.Throw);

        // If at any point the player stops holding the interact button, or we're not in the game state anymore -> stop interacting
        if (interactPressed || throwPressed || LevelManager.Instance.GameState != LevelManager.State.Game)
        {
            StopInteracting(currentTargetInteractable);
            return;
        }

        if (InteractionTimer.IsRunning)
        {
            float t = currentInteractionDuration - InteractionTimer.RemainingTime(Runner).Value;
            progressBar.UpdateProgress(t / currentInteractionDuration);
        }

        // We interacted with the object -> Reset everything and call the interact function
        if (InteractionTimer.Expired(Runner))
        {
            Interactable target = currentTargetInteractable;
            StopInteracting(target);
            
            ExecuteInteraction(target);
        }
    }

    private void ExecuteInteraction(Interactable target)
    {
        if (!HasStateAuthority) return;

        // If I'm the server I call the interact function directly
        // If I'm the client I need to send an RPC to the server to call the interact function for me
        if (target.executionTarget == Interactable.ExecutionTarget.ClientSide)
            RPC_ClientInteract(target.NetworkId);
        else
            target.Interact(this);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RPC_ClientInteract(int targetInteractableId)
    {
        if (InteractableRegistry.All.TryGetValue(targetInteractableId, out Interactable targetInteractable))
            targetInteractable.Interact(this);
    }

    private void StopInteracting(Interactable insideInteractable)
    {
        playerAnimationController.EndInteraction(); // TODO fix bad animation coupling
        Interacting = false;
        insideInteractable.IsAlreadyInteractedWith = false;

        progressBar.ResetProgress();
        currentTargetInteractable = null;
        InteractionTimer = TickTimer.None;
    }

    /// <summary>
    /// Discards the item currently held
    /// </summary>
    public void ConsumeCurrentItem()
    {
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

    private void OnGameEndedOrReturnedToLobby()
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
        LevelManager.Instance.GameEndedOrReturnedToLobby -= OnGameEndedOrReturnedToLobby;
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
