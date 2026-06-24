using Fusion;
using LitMotion;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public List<Interactable> insideInteractableList { get; } = new();
    public bool IsHolding => SyncHeldItemType != -1 && HeldItem != null;
    public Item HeldItem { get; set; }

    [SerializeField] private float minThrowSpeed = 40f;
    [SerializeField] private float maxThrowSpeed = 70f;
    [SerializeField] private float aimChargeDuration = .5f;
    private float aimSpeedRatioVelocity;
    [SerializeField] private float timeBeforeAimCharge = .15f;

    [Header("References")]

    [SerializeField] private PlayerTeam playerTeam;
    public PlayerTeam PlayerTeam => playerTeam;

    [SerializeField] private PlayerControlBadge playerControlBadge;
    public PlayerControlBadge PlayerControlBadge => playerControlBadge;

    [SerializeField] private PlayerBadge playerBadge;
    public PlayerBadge PlayerBadge => playerBadge;

    [SerializeField] private PlayerMovement playerMovement;
    public PlayerMovement PlayerMovement => playerMovement;

    [SerializeField] private PlayerInputPoller inputPoller;
    public PlayerInputPoller InputPoller => inputPoller;

    [SerializeField] private PlayerStats playerStats;
    public PlayerStats PlayerStats => playerStats;
    [SerializeField] private PlayerInitPosition playerInitPosition;
    public PlayerInitPosition PlayerInitPosition => playerInitPosition;

    [SerializeField] private ProgressBar progressBar;

    [SerializeField] private PlayerAnimationController playerAnimationController;  // TODO remove reference, fix bad animation coupling
    [SerializeField] private Transform itemParent;

    public Action GrabbedNewItem;

    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [Networked] public TickTimer InteractionTimer { get; set; }

    [Networked, OnChangedRender(nameof(OnTargetChanged))]
    public int SyncTargetId { get; set; } = -1;

    [Networked, OnChangedRender(nameof(OnHeldItemChanged))]
    public int SyncHeldItemType { get; set; } = -1;

    [Networked, OnChangedRender(nameof(OnInteractionChanged))]
    public NetworkBool SyncIsInteracting { get; set; }
    [Networked] public float SyncThrowSpeedRatio { get; set; }
    [Networked] public Vector2 SyncThrowDirection { get; set; }

    public enum AimingState { NotAiming, StartingToAim, AimingLockedIn }
    [Networked] public AimingState CurrentAimingState { get; set; }
    [Networked] private float TimerBeforeAimCharge { get; set; }

    private float ThrowSpeed => SyncThrowSpeedRatio * (maxThrowSpeed - minThrowSpeed) + minThrowSpeed;
    public Vector2 ThrowVelocity => ThrowSpeed * SyncThrowDirection;

    private PlayerData currentTickData;

    private MotionHandle grabbingLerp;
    private MotionHandle rotationLerp;

    public bool LockedInSettingsMenu { get; private set; }

    public event Action AvatarSpawned;
    public event Action StartedAimingLockedIn;
    public event Action StoppedAiming;

    public static event Action<Player> PlayerSpawned;
    public static event Action<Player> PlayerDespawned;

    private void Awake()
    {
        aimSpeedRatioVelocity = 1 / aimChargeDuration;
    }

    private void Start()
    {
        LevelManager.GameEnded += OnGameEnded;
    }

    public override void Spawned()
    {
        PlayerRegistry.Register(this);
        PlayerSpawned?.Invoke(this);
        AvatarSpawned?.Invoke();

        if (SyncTargetId != -1 && InteractableRegistry.All.TryGetValue(SyncTargetId, out var target))
        {
            target.RefreshHighlight();
            target.OnTargetedBy(this, true);
        }

        CurrentAimingState = AimingState.NotAiming;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        PlayerRegistry.Unregister(this);
        PlayerDespawned?.Invoke(this);

        if (SyncTargetId != -1 && InteractableRegistry.All.TryGetValue(SyncTargetId, out var target))
        {
            target.RefreshHighlight();
            target.OnTargetedBy(this, false);
        }

        insideInteractableList.Clear();
        ConsumeCurrentItem();
    }

    public override void FixedUpdateNetwork()
    {
        if (!SyncIsInteracting)
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

            HandleInput();

            PreviousButtons = myData.Buttons;
        }

        playerAnimationController.HasItem(SyncHeldItemType != -1); // TODO fix bad animation coupling
    }

    private void HandleInput()
    {
        switch (LevelManager.Instance.GameState)
        {
            case LevelManager.State.Game:
                if (SyncIsInteracting)
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
                SyncThrowSpeedRatio = 0f;
                TimerBeforeAimCharge = 0f;
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
        if (HasStateAuthority || HasInputAuthority)
            SyncThrowDirection = playerMovement.LastNonZeroInput;

        bool interactReleased = currentTickData.Buttons.WasReleased(PreviousButtons, PlayerInputButtons.Interact);
        bool throwReleased = currentTickData.Buttons.WasReleased(PreviousButtons, PlayerInputButtons.Throw);
        bool throwIsPressed = currentTickData.Buttons.IsSet(PlayerInputButtons.Throw);

        if (throwReleased)
        {
            ThrowAndExitAim(ThrowVelocity);
            return;
        }

        // If the player releases the interact button BUT is still holding the throw button at the same time,
        // their intention is probably to throw the item, so we should not drop it and stay in the StartingToAim state.
        if (interactReleased && !throwIsPressed)
        {
            ThrowAndExitAim();
            return;
        }
        
        TimerBeforeAimCharge += Runner.DeltaTime;
        if (TimerBeforeAimCharge >= timeBeforeAimCharge)
        {
            CurrentAimingState = AimingState.AimingLockedIn;
            StartedAimingLockedIn?.Invoke();
        }
    }

    private void ThrowAndExitAim() => ThrowAndExitAim(Vector2.zero);
    private void ThrowAndExitAim(Vector2 throwVelocity)
    {
        SyncThrowSpeedRatio = 0f;
        CurrentAimingState = AimingState.NotAiming;

        StoppedAiming?.Invoke();
        TryDropHeldItem(throwVelocity);
    }

    private void HandleInputAimingLockedIn()
    {
        if (HasStateAuthority || HasInputAuthority)
        {
            SyncThrowSpeedRatio = Mathf.Clamp01(SyncThrowSpeedRatio + aimSpeedRatioVelocity * Runner.DeltaTime);
            SyncThrowDirection = playerMovement.LastNonZeroInput;
        }

        bool interactReleased = currentTickData.Buttons.WasReleased(PreviousButtons, PlayerInputButtons.Interact);
        bool throwReleased = currentTickData.Buttons.WasReleased(PreviousButtons, PlayerInputButtons.Throw);

        if (interactReleased || throwReleased)
            ThrowAndExitAim(ThrowVelocity);
    }

    public void LocalEnterInteractable(Interactable interactable)
    {
        if (interactable != null && !insideInteractableList.Contains(interactable))
            insideInteractableList.Add(interactable);
    }

    public void LocalExitInteractable(Interactable interactable)
    {
        if (interactable != null)
            insideInteractableList.Remove(interactable);
    }

    private void UpdateClosestInteractable()
    {
        if (!HasStateAuthority) 
            return;

        // trust me some crazy shit happen when logic is shared across a network
        insideInteractableList.RemoveAll(i => i == null || !i.gameObject.activeInHierarchy);

        Interactable localClosest = insideInteractableList.Count > 0 ? GetClosestInteractable() : null;
        int newTargetId = localClosest != null ? localClosest.NetworkId : -1;

        if (SyncTargetId != newTargetId)
            SyncTargetId = newTargetId;
    }
    private bool TryInteract()
    {
        Interactable target = null;
        if (SyncTargetId != -1)
            InteractableRegistry.All.TryGetValue(SyncTargetId, out target);

        if (target == null)
            return false;

        bool canBeHighlighted = target.CheckIfCanBeHighlighted(this);
        float time = target.GetInteractionTime();

        if (time > 0)
        {
            // Before the network update it was a couroutine
            InteractionTimer = TickTimer.CreateFromSeconds(Runner, time);

            if (HasStateAuthority || HasInputAuthority) 
                SyncIsInteracting = true;
        }
        else
            ExecuteInteraction(target);

        return canBeHighlighted;
    }

    private void ProcessLongInteraction()
    {
        bool interactReleased = currentTickData.Buttons.WasReleased(PreviousButtons, PlayerInputButtons.Interact);
        bool throwReleased = currentTickData.Buttons.WasReleased(PreviousButtons, PlayerInputButtons.Throw);

        Interactable target = null;
        if (SyncTargetId != -1)
            InteractableRegistry.All.TryGetValue(SyncTargetId, out target);

        // If at any point the player stops holding the interact button, or we're not in the game state anymore -> stop interacting
        if (interactReleased || throwReleased || LevelManager.Instance.GameState != LevelManager.State.Game || target == null)
        {
            StopInteracting(target);
            return;
        }

        // We interacted with the object -> Reset everything and call the interact function
        if (InteractionTimer.Expired(Runner))
        {
            StopInteracting(target);   
            ExecuteInteraction(target);
        }
    }

    public override void Render()
    {
        if (!SyncIsInteracting || !InteractionTimer.IsRunning)
            return;

        if (SyncTargetId != -1 && InteractableRegistry.All.TryGetValue(SyncTargetId, out var target))
        {
            float duration = target.GetInteractionTime();
            if (duration > 0)
            {
                float t = duration - InteractionTimer.RemainingTime(Runner).GetValueOrDefault();
                progressBar.UpdateProgress(t / duration);
            }
        }
    }

    private void ExecuteInteraction(Interactable target)
    {
        if (!HasStateAuthority) return;

        // If I'm the host I call the interact function directly
        // If I'm the client I need to send an RPC to the host to call the interact function for me
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
        InteractionTimer = TickTimer.None;

        if (HasStateAuthority || HasInputAuthority)
            SyncIsInteracting = false;
    }

    private void OnHeldItemChanged(NetworkBehaviourBuffer previous)
    {
        int previousItemType = GetPropertyReader<int>(nameof(SyncHeldItemType)).Read(previous);

        if (SyncHeldItemType == -1 && previousItemType != -1)
            playerAnimationController.Drop();

        else if (SyncHeldItemType != -1 && previousItemType == -1)
            playerAnimationController.Grab();
    }

    private void OnTargetChanged(NetworkBehaviourBuffer previous)
    {
        int previousTargetId = GetPropertyReader<int>(nameof(SyncTargetId)).Read(previous);

        if (previousTargetId != -1 && InteractableRegistry.All.TryGetValue(previousTargetId, out var oldInteractable))
        {
            oldInteractable.RefreshHighlight();
            oldInteractable.OnTargetedBy(this, false);
        }
            
        if (SyncTargetId != -1 && InteractableRegistry.All.TryGetValue(SyncTargetId, out var newInteractable))
        {
            newInteractable.RefreshHighlight();
            newInteractable.OnTargetedBy(this, true);
        }
    }

    private void OnInteractionChanged(NetworkBehaviourBuffer previous)
    {
        if (SyncIsInteracting)
        {
            if (SyncTargetId != -1 && InteractableRegistry.All.TryGetValue(SyncTargetId, out var target))
            {
                if (target is Workbench) 
                    playerAnimationController.StartCutting();     // TODO fix bad animation coupling
                else if (target is Collector) 
                    playerAnimationController.StartCollecting();  // TODO fix bad animation coupling
                else
                    Debug.LogError("This Interactable is not currently supported by the animator");
                //no interactable in the game takes time aside from Collector and Workbench as of rn
            }
            progressBar.StartProgress();
        }
        else
        {
            playerAnimationController.EndInteraction();  // TODO fix bad animation coupling
            progressBar.ResetProgress();
        }
    }

    /// <summary>
    /// Discards the item currently held
    /// </summary>
    public void ConsumeCurrentItem()
    {
        if (HasStateAuthority || HasInputAuthority)
            SyncHeldItemType = -1;

        if (HasStateAuthority && HeldItem != null)
        {
            if (HeldItem.TryGetComponent(out NetworkObject netObj))
                Runner.Despawn(netObj);
            
            HeldItem = null;
        }
    }

    public bool TryDropHeldItem() => TryDropHeldItem(Vector2.zero);
    
    /// <summary>
    /// If holding an item, drops to the ground the item currently held
    /// </summary>
    public bool TryDropHeldItem(Vector2 currentThrowSpeed)
    {
        if (!IsHolding || HeldItem.State != Item.ItemState.Held)
            return false;

        if (HasStateAuthority || HasInputAuthority)
            SyncHeldItemType = -1;

        grabbingLerp.TryCancel();
        rotationLerp.TryCancel();

        if (HasStateAuthority && HeldItem != null)
        {
            HeldItem.State = Item.ItemState.Transitioning;
            HeldItem.Drop(currentThrowSpeed);
            HeldItem = null;
        }

        return true;
    }

    /// <summary>
    /// Makes the player grab a newly instantiated Item
    /// </summary>
    /// <param name="itemPrefab"></param>
    /// <param name="originallyCollectedByTeam">The team this item was originally collected by. If left null this will be set as this player's team</param>
    public void GrabNewItem(Item itemPrefab, PlayerTeam.Team? originallyCollectedByTeam = null)
    {
        if (!HasStateAuthority) return;

        NetworkObject netObj = Runner.Spawn(itemPrefab.gameObject, transform.position, Quaternion.identity);
        Item itemInstance = netObj.GetComponent<Item>();
        NetworkItem netItem = itemInstance.NetworkItem;

        netItem.SyncOriginalTeam = originallyCollectedByTeam ?? playerTeam.CurrentTeam;
        netItem.Grab(this, false);

        GrabbedNewItem?.Invoke();
    }

    public void GrabItem(Item item, bool interpolatePosition)
    {
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

        if (HasStateAuthority || HasInputAuthority)
            SyncHeldItemType = (int)item.ItemType;
    }

    private void OnGameEnded()
    {
        SyncIsInteracting = false;
        SyncThrowSpeedRatio = 0f;
        CurrentAimingState = AimingState.NotAiming;

        StoppedAiming?.Invoke();
        ConsumeCurrentItem();
    }
    
    private void OnDisable()
    {
        LevelManager.GameEnded -= OnGameEnded;
    }

    private Interactable GetClosestInteractable()
    {
        Interactable closest = null;
        float minSqrDistance = float.MaxValue;

        foreach (Interactable interactable in insideInteractableList)
        {
            if (HeldItem != null && interactable.gameObject == HeldItem.gameObject)
                continue;

            bool isAvailable = true;
            foreach (Player p in PlayerRegistry.All) 
            {
                if (p != this && p.SyncIsInteracting && p.SyncTargetId == interactable.NetworkId)
                {
                    isAvailable = false;
                    break;
                }
            }

            float sqrDistance = ((Vector2)interactable.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (sqrDistance < minSqrDistance && isAvailable && interactable.CanInteract(this))
            {
                minSqrDistance = sqrDistance;
                closest = interactable;
            }
        }

        return closest;
    }
}
