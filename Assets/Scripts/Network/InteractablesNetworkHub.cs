using Fusion;
using UnityEngine;

public class InteractablesNetworkHub : NetworkBehaviour
{
    public static InteractablesNetworkHub Instance { get; private set; }

    public override void Spawned() => Instance = this;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncWorkbenchState(int interactableId, Workbench.State newState, PlayerTeam.Team team)
    {
        if (InteractableRegistry.All.TryGetValue(interactableId, out Interactable target) && target is Workbench workbench)
            workbench.ApplyState(newState, team);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncFurnaceState(int interactableId, Furnace.State newState, PlayerTeam.Team team)
    {
        if (InteractableRegistry.All.TryGetValue(interactableId, out Interactable target) && target is Furnace furnace)
            furnace.ApplyState(newState, team);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncTowerBuild(int interactableId, Item.Type itemType)
    {
        if (InteractableRegistry.All.TryGetValue(interactableId, out Interactable target) && target is Tower tower)
            tower.ApplyConstructPiece(itemType);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SyncTowerError(int interactableId)
    {
        if (InteractableRegistry.All.TryGetValue(interactableId, out Interactable target) && target is Tower tower)
            tower.WrongItemError();
    }
}