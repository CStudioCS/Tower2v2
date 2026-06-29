using System;
using System.Collections;
using UnityEngine;
using static PlayerTeam;

public class Furnace : Interactable
{
    [Header("Furnace")]
    [SerializeField] private float cookTime = 4;
    [SerializeField] private Item brickItemPrefab;
    [SerializeField] private ProgressBar progressBar;

    private PlayerTeam.Team itemCookedByTeam;

    private State state;
    public State NetworkState
    {
        get
        {
            if (InteractablesNetworkHub.Instance != null && InteractablesNetworkHub.Instance.SyncStates.TryGet(NetworkId, out var data))
                return (State)data.StateValue;

            return State.Empty;
        }
    }

    public enum State { Empty, Cooking, Cooked }

    public event Action StartedCooking;
    public event Action StoppedCooking;

    [Header("Furnace Color")]
    [SerializeField] private bool allowLeftTeam = true;
    [SerializeField] private bool allowRightTeam = true;

    private int fireSoundIndex = -1;
    private Coroutine cookingCoroutine;

    private int lastProcessedActionCounter = 0;
    private bool isPredicting = false;

    public bool Allowed(Player player) => Allowed(player.PlayerTeam.CurrentTeam);
    private bool Allowed(PlayerTeam.Team team) => team switch
    {
        PlayerTeam.Team.Left => allowLeftTeam,
        PlayerTeam.Team.Right => allowRightTeam,
        _ => true
    };

    protected override void Awake()
    {
        base.Awake();
        state = State.Empty;
    }

    public override bool CanInteract(Player player)
    {
        if (!LevelManager.InGame)
            return false;
        if (!Allowed(player))
            return false;

        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.ItemType == Item.Type.Clay;
            case State.Cooking:
                return false;
            case State.Cooked:
                return !player.IsHolding;
            default:
                throw new UnityException("Furnace state not handled in CanInteract");
        }
    }
    public void PutClayIn(PlayerTeam.Team team)
        => InteractablesNetworkHub.Instance?.SetInteractableState(NetworkId, (int)State.Cooking, (int)team);

    public override void Interact(Player player)
    {
        bool isHost = InteractablesNetworkHub.Instance?.HasStateAuthority == true;

        bool isTryingToPutClay = player.IsHolding && player.HeldItem.ItemType == Item.Type.Clay;
        bool isTryingToTakeBrick = !player.IsHolding;

        switch (state)
        {
            case State.Empty:
                if (!isTryingToPutClay)
                    break;

                if (isHost && NetworkState != State.Empty)
                {
                    InteractablesNetworkHub.Instance.RejectInteractableAction(NetworkId);
                    break;
                }

                if (player.HasInputAuthority)
                    isPredicting = true;

                ApplyState(State.Cooking, player.PlayerTeam.CurrentTeam);
                player.ConsumeCurrentItem();

                if (isHost)
                    PutClayIn(player.PlayerTeam.CurrentTeam);

                break;

            case State.Cooked:
                if (!isTryingToTakeBrick)
                    break;

                if (isHost && NetworkState != State.Cooked)
                {
                    InteractablesNetworkHub.Instance.RejectInteractableAction(NetworkId);
                    break;
                }

                if (player.HasInputAuthority)
                    isPredicting = true;
                    
                ApplyState(State.Empty, itemCookedByTeam);
                player.GrabNewItem(brickItemPrefab, itemCookedByTeam);

                if (isHost)
                {
                    player.PlayerStats.BricksCooked++;
                    InteractablesNetworkHub.Instance?.SetInteractableState(NetworkId, (int)State.Empty, (int)player.PlayerTeam.CurrentTeam);
                }
                break;
        }
    }

    public void ApplyState(State newState, PlayerTeam.Team team)
    {
        State oldState = state;
        state = newState;
        itemCookedByTeam = team;

        if (oldState == State.Cooking && newState != State.Cooking)
            StopCookingVisuals();

        switch (newState)
        {
            case State.Cooking:
                if (oldState != State.Cooking)
                    cookingCoroutine = StartCoroutine(VisualCookRoutine());
                break;

            case State.Cooked:
                progressBar.SetProgressMax();
                break;

            case State.Empty:
                progressBar.ResetProgress();
                if (oldState == State.Cooked)
                    SoundManager.instance.PlaySound("FurnaceBricks");
                break;
        }
    }

    private IEnumerator VisualCookRoutine()
    {
        StartedCooking?.Invoke();
        fireSoundIndex = SoundManager.instance.PlaySound("FurnaceFire");
        progressBar.StartProgress();

        float t = 0;
        while (t < cookTime && state == State.Cooking)
        {
            progressBar.UpdateProgress(t / cookTime);
            t += Time.deltaTime;
            yield return null;
        }

        // The host decides when the time is up and validates the brick for everyone
        if (state == State.Cooking && InteractablesNetworkHub.Instance?.HasStateAuthority == true)
            InteractablesNetworkHub.Instance.SetInteractableState(NetworkId, (int)State.Cooked, (int)itemCookedByTeam);
    }

    private void StopCookingVisuals()
    {
        if (cookingCoroutine != null)
        {
            StopCoroutine(cookingCoroutine);
            cookingCoroutine = null;
        }

        if (fireSoundIndex != -1)
        {
            SoundManager.instance.StopSound(fireSoundIndex);
            fireSoundIndex = -1;
        }

        StoppedCooking?.Invoke();
    }

    public override float GetInteractionTime() => 0;
    
    protected override void OnGameEnded()
    {
        base.OnGameEnded();
        state = State.Empty;
        StopCookingVisuals();
        progressBar.ResetProgress();
    }

    public override void SyncWithNetworkState(InteractableState data)
    {
        if (data.ActionCounter > lastProcessedActionCounter)
        {
            lastProcessedActionCounter = data.ActionCounter;
            isPredicting = false;

            State networkState = (State)data.StateValue;
            PlayerTeam.Team networkTeam = (PlayerTeam.Team)data.TeamId;

            // Validation
            if (state == networkState)
                itemCookedByTeam = networkTeam;
            // Rollback
            else
                ApplyState(networkState, networkTeam);

        }
        else if (!isPredicting)
        {
            State networkState = (State)data.StateValue;
            if (state != networkState)
                ApplyState(networkState, (PlayerTeam.Team)data.TeamId);
        }
    }
}
