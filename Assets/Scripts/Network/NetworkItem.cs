using Fusion;
using UnityEngine;

public class NetworkItem : NetworkBehaviour
{
    [SerializeField] private Item item;

    [Networked] public PlayerTeam.Team SyncOriginalTeam { get; set; }
    [Networked] public NetworkBool SyncAnimateGrab { get; set; }

    [Networked, OnChangedRender(nameof(OnStateOrOwnerChanged))]
    public int SyncState { get; set; }

    [Networked, OnChangedRender(nameof(OnStateOrOwnerChanged))]
    public Player SyncOwner { get; set; }

    public override void Spawned()
    {
        item.RegisterNetworkId((int)Object.Id.Raw);

        if (HasStateAuthority && SyncOwner == null)
            SyncState = (int)Item.ItemState.Dropped;

        OnStateOrOwnerChanged();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
        => InteractableRegistry.Unregister(item);

    // Called by all clients (and host) when SyncState or SyncOwnerId changes
    private void OnStateOrOwnerChanged() => item.ApplyNetworkState((Item.ItemState)SyncState, SyncOwner, SyncOriginalTeam);

    public void Grab(Player player, bool interpolatePosition)
    {
        if (!HasStateAuthority) return;

        SyncAnimateGrab = interpolatePosition;
        SyncOwner = player;
        SyncState = (int)Item.ItemState.Held;
    }

    public void Drop()
    {
        if (!HasStateAuthority) return;
        SyncOwner = null;
        SyncState = (int)Item.ItemState.Dropped;
    }
}