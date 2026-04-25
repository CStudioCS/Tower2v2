using Fusion.Addons.Physics;
using UnityEngine;

public class PlayerItemCatcher : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float minimumCatchingVelocity = 0.5f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (!player.HasStateAuthority) 
            return;

        if (player.SyncIsInteracting)
            return;

        if (collider == null || collider.transform.parent == null)
            return;

        if (!collider.transform.parent.TryGetComponent(out Item item) || !collider.transform.parent.TryGetComponent(out NetworkRigidbody2D nrb))
            return;

        if (item.LastOwner == null || item.LastOwner == player)
            return;

        if (item.LastOwner.PlayerTeam.CurrentTeam != player.PlayerTeam.CurrentTeam)
            return;

        if (item.State != Item.ItemState.Dropped)
            return;

        if (Mathf.Abs(nrb.Rigidbody.linearVelocity.magnitude) < minimumCatchingVelocity)
            return;

        if (player.IsHolding)
            return;

        item.NetworkItem.Grab(player, false);
    }
}