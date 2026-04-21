using UnityEngine;

public class Workbench : Interactable
{
    public enum State { Empty, HasWoodLog, HasWoodPlank }
    private State state;
    public State WorkbenchState => state;

    [SerializeField] private float putOrPickUpItemInteractionTime = 0f;
    [SerializeField] private float cutWoodInteractionTime = 1f;
    private float currentInteractionTime;

    [Header("References")]
    [SerializeField] private Item woodPlankItemPrefab;

    private PlayerTeam.Team cutLastByTeam;

    [SerializeField] private GameObject woodOnTable;
    [SerializeField] private GameObject woodPlanckOnTable;
    [SerializeField] private GameObject axe;

    private int soundIndex = -1;

    private void Awake()
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
            case State.HasWoodPlank:
                return !player.IsHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    public void PutWoodLog() => InteractablesNetworkHub.Instance.RPC_SyncWorkbenchState(NetworkId, State.HasWoodLog, cutLastByTeam);

    public override void Interact(Player player)
    {
        switch (state)
        {
            case State.Empty:
                InteractablesNetworkHub.Instance.RPC_SyncWorkbenchState(NetworkId, State.HasWoodLog, cutLastByTeam);
                player.ConsumeCurrentItem();
                break;

            case State.HasWoodLog:
                player.PlayerStats.woodCut++;
                InteractablesNetworkHub.Instance.RPC_SyncWorkbenchState(NetworkId, State.HasWoodPlank, player.PlayerTeam.CurrentTeam);
                break;

            case State.HasWoodPlank:
                InteractablesNetworkHub.Instance.RPC_SyncWorkbenchState(NetworkId, State.Empty, cutLastByTeam);
                player.GrabNewItem(woodPlankItemPrefab, cutLastByTeam);  //ownership for wood is determined by who cut it, not who collected it 
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
        woodPlanckOnTable.SetActive(false);
        axe.SetActive(false);

        switch (newState)
        {
            case State.Empty:
                axe.SetActive(true);
                if (oldState == State.HasWoodPlank)
                    SoundManager.instance.PlaySound("WoodSound"); 
                break;

            case State.HasWoodLog:
                woodOnTable.SetActive(true);
                if (oldState == State.Empty)
                    SoundManager.instance.PlaySound("WoodSound");
                break;

            case State.HasWoodPlank:
                woodPlanckOnTable.SetActive(true);
                break;
        }
    }

    public override float GetInteractionTime() => currentInteractionTime;
    
    protected override void OnGameEndedOrReturnedToLobby()
    {
        base.OnGameEndedOrReturnedToLobby();
        state = State.Empty;
        ResetGraphicsOnTable();
    }

    private void ResetGraphicsOnTable()
    {
        woodOnTable.SetActive(false);
        woodPlanckOnTable.SetActive(false);
        axe.SetActive(true);
    }

    private void Update()
    {
        if(IsAlreadyInteractedWith && soundIndex == -1)
        {
            soundIndex = SoundManager.instance.PlaySound("Hammer");
        }

        if (!IsAlreadyInteractedWith && soundIndex != -1)
        {
            SoundManager.instance.StopSound(soundIndex);
            soundIndex = -1;
        }
    }
}
