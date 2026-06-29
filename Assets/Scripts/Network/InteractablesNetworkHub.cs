using Fusion;
using UnityEngine;
using static Fusion.NetworkBehaviour;

public struct InteractableState : INetworkStruct
{
    public int StateValue;
    public int TeamId;
    public int ItemType;
    public int ActionCounter;
}

public class InteractablesNetworkHub : NetworkBehaviour
{
    public static InteractablesNetworkHub Instance { get; private set; }

    [Networked, Capacity(30)] // Assuming a maximum of 30 interactables in the game
    public NetworkDictionary<int, InteractableState> SyncStates => default;
    private ChangeDetector changeDetector;

    public override void Spawned()
    {
        Instance = this;
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetInteractableState(int networkId, int stateValue, int teamId = -1, int itemType = -1)
    {
        int currentCounter = 0;
        if (SyncStates.TryGet(networkId, out var oldState))
            currentCounter = oldState.ActionCounter;

        var newState = new InteractableState
        {
            StateValue = stateValue,
            TeamId = teamId,
            ItemType = itemType,
            ActionCounter = currentCounter + 1
        };

        SyncStates.Set(networkId, newState);
    }

    public override void Render()
    {
        foreach (var propertyName in changeDetector.DetectChanges(this))
        {
            if (propertyName == nameof(SyncStates))
                NotifyInteractablesOfChanges();
        }
    }

    private void NotifyInteractablesOfChanges()
    {
        foreach (var kvp in SyncStates)
        {
            if (InteractableRegistry.All.TryGetValue(kvp.Key, out Interactable interactable))
                interactable.SyncWithNetworkState(kvp.Value);
        }
    }

    public void RejectInteractableAction(int networkId)
    {
        if (!HasStateAuthority) return;

        if (SyncStates.TryGet(networkId, out var currentState))
        {
            currentState.ActionCounter++;
            SyncStates.Set(networkId, currentState);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    public void RPC_SyncTowerError(int interactableId)
    {
        if (InteractableRegistry.All.TryGetValue(interactableId, out Interactable target) && target is Tower tower)
            tower.WrongItemError();
    }
}