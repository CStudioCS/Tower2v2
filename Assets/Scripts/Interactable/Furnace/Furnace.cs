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
    public State FurnaceState => state;
    public enum State { Empty, Cooking, Cooked }

    public event Action StartedCooking;
    public event Action StoppedCooking;

    [Header("Furnace Color")]
    [SerializeField] private bool allowLeftTeam = true;
    [SerializeField] private bool allowRightTeam = true;

    private int fireSoundIndex = -1;
    private Coroutine cookingCoroutine;

    public bool Allowed(Player player) => Allowed(player.PlayerTeam.CurrentTeam);
    private bool Allowed(PlayerTeam.Team team) => team switch
    {
        PlayerTeam.Team.Left => allowLeftTeam,
        PlayerTeam.Team.Right => allowRightTeam,
        _ => true
    };
    
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
    public void PutClayIn(PlayerTeam.Team team) => InteractablesNetworkHub.Instance.RPC_SyncFurnaceState(NetworkId, State.Cooking, team);

    public override void Interact(Player player)
    {
        switch (state)
        {
            case State.Empty:
                InteractablesNetworkHub.Instance.RPC_SyncFurnaceState(NetworkId, State.Cooking, player.PlayerTeam.CurrentTeam);
                player.ConsumeCurrentItem();
                break;

            case State.Cooked:
                InteractablesNetworkHub.Instance.RPC_SyncFurnaceState(NetworkId, State.Empty, itemCookedByTeam);
                player.GrabNewItem(brickItemPrefab, itemCookedByTeam);
                player.PlayerStats.BricksCooked++;
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
        if (state == State.Cooking && InteractablesNetworkHub.Instance.HasStateAuthority)
            InteractablesNetworkHub.Instance.RPC_SyncFurnaceState(NetworkId, State.Cooked, itemCookedByTeam);
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
}
