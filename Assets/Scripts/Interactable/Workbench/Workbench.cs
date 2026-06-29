using UnityEngine;

public class Workbench : Interactable
{
    public enum State { Empty, HasWoodLog }
    private State state;
    public State NetworkState
    {
        get
        {
            if (InteractablesNetworkHub.Instance != null && 
                InteractablesNetworkHub.Instance.SyncStates.TryGet(NetworkId, out var data))
                    return (State)data.StateValue;

            return State.Empty;
        }
    }

    [SerializeField] private float putOrPickUpItemInteractionTime = 0f;
    [SerializeField] private float cutWoodInteractionTime = 1f;
    private float currentInteractionTime;
    private PlayerTeam.Team cutLastByTeam;

    [Header("References")]
    [SerializeField] private Item woodPlankItemPrefab;
    [SerializeField] private GameObject woodOnTable;
    [SerializeField] private GameObject axe;

    private int soundIndex = -1;
    private int lastProcessedActionCounter = 0;
    private bool isPredicting = false;

    protected override void Awake()
    {
        base.Awake();
        state = State.Empty;
        ApplyVisualState(State.Empty, State.Empty);
    }

    public override bool CanInteract(Player player)
    {
        if (!LevelManager.InGame)
            return false;

        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.ItemType == Item.Type.WoodLog;
            case State.HasWoodLog:
                return !player.IsHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    public void PutWoodLog(PlayerTeam.Team team) 
        => InteractablesNetworkHub.Instance?.SetInteractableState(NetworkId, (int)State.HasWoodLog, (int)team);

    public override void Interact(Player player)
    {
        bool isHost = InteractablesNetworkHub.Instance?.HasStateAuthority == true;

        bool isTryingToPutLog = player.IsHolding && player.HeldItem.ItemType == Item.Type.WoodLog;
        bool isTryingToCut = !player.IsHolding;

        switch (state)
        {
            case State.Empty:
                if (!isTryingToPutLog)
                    break;

                if (isHost && NetworkState != State.Empty)
                {
                    InteractablesNetworkHub.Instance.RejectInteractableAction(NetworkId);
                    break;
                }

                if (player.HasInputAuthority)
                    isPredicting = true;

                ApplyState(State.HasWoodLog, player.PlayerTeam.CurrentTeam);
                player.ConsumeCurrentItem();

                if (isHost)
                    PutWoodLog(player.PlayerTeam.CurrentTeam);
                    
                break;

            case State.HasWoodLog:
                if (!isTryingToCut)
                    break;

                if (isHost && NetworkState != State.HasWoodLog)
                {
                    InteractablesNetworkHub.Instance.RejectInteractableAction(NetworkId);
                    break;
                }

                if (player.HasInputAuthority)
                    isPredicting = true;

                ApplyState(State.Empty, cutLastByTeam);
                player.GrabNewItem(woodPlankItemPrefab, cutLastByTeam);

                if (isHost)
                {
                    player.PlayerStats.WoodCut++;
                    InteractablesNetworkHub.Instance.SetInteractableState(NetworkId, (int)State.Empty, (int)player.PlayerTeam.CurrentTeam);
                }
                break;
        }
    }

    public void ApplyState(State newState, PlayerTeam.Team team)
    {
        State oldState = state;
        state = newState;
        cutLastByTeam = team;

        currentInteractionTime = (state == State.HasWoodLog) ? cutWoodInteractionTime : putOrPickUpItemInteractionTime;

        ApplyVisualState(oldState, newState);
    }

    private void ApplyVisualState(State oldState, State newState)
    {
        woodOnTable.SetActive(false);
        axe.SetActive(false);

        switch (newState)
        {
            case State.Empty:
                axe.SetActive(true);
                if (oldState == State.HasWoodLog)
                    SoundManager.instance.PlaySound("WoodSound"); 
                break;

            case State.HasWoodLog:
                woodOnTable.SetActive(true);
                if (oldState == State.Empty)
                    SoundManager.instance.PlaySound("WoodSound");
                break;
        }
    }

    public override float GetInteractionTime() => currentInteractionTime;
    
    protected override void OnGameEnded()
    {
        base.OnGameEnded();
        if (InteractablesNetworkHub.Instance?.HasStateAuthority == true)
            InteractablesNetworkHub.Instance.SetInteractableState(NetworkId, (int)State.Empty, (int)cutLastByTeam);
    }

    private void Update()
    {
        if (IsAlreadyInteractedWith() && soundIndex == -1)
            soundIndex = SoundManager.instance.PlaySound("Hammer");

        if (!IsAlreadyInteractedWith() && soundIndex != -1)
        {
            SoundManager.instance.StopSound(soundIndex);
            soundIndex = -1;
        }
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
                cutLastByTeam = networkTeam;
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
