using UnityEngine;

public class Workbench : Interactable
{
    public enum State { Empty, HasWoodLog }
    private State state;
    public State WorkbenchState => state;

    [SerializeField] private float putOrPickUpItemInteractionTime = 0f;
    [SerializeField] private float cutWoodInteractionTime = 1f;
    private float currentInteractionTime;

    [Header("References")]
    [SerializeField] private Item woodPlankItemPrefab;

    private PlayerTeam.Team cutLastByTeam;

    [SerializeField] private GameObject woodOnTable;
    [SerializeField] private GameObject axe;

    private int soundIndex = -1;

    protected override void Awake()
    {
        base.Awake();
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

    public void PutWoodLog()
    {
        ApplyState(State.HasWoodLog, cutLastByTeam);
        InteractablesNetworkHub.Instance.RPC_SyncWorkbenchState(NetworkId, State.HasWoodLog, cutLastByTeam);
    }

    public override void Interact(Player player)
    {
        switch (state)
        {
            case State.Empty:
                PutWoodLog();
                player.ConsumeCurrentItem();
                break;

            case State.HasWoodLog:
                player.PlayerStats.WoodCut++;
                ApplyState(State.Empty, player.PlayerTeam.CurrentTeam);
                InteractablesNetworkHub.Instance.RPC_SyncWorkbenchState(NetworkId, State.Empty, player.PlayerTeam.CurrentTeam);
                player.GrabNewItem(woodPlankItemPrefab, cutLastByTeam);
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
        ApplyState(State.Empty, cutLastByTeam);
    }

    private void Update()
    {
        if(IsAlreadyInteractedWith() && soundIndex == -1)
        {
            soundIndex = SoundManager.instance.PlaySound("Hammer");
        }

        if (!IsAlreadyInteractedWith() && soundIndex != -1)
        {
            SoundManager.instance.StopSound(soundIndex);
            soundIndex = -1;
        }
    }
}
