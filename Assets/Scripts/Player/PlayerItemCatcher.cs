using UnityEngine;

public class PlayerItemCatcher : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float minimumCatchingVelocity = 0.5f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (player.Interacting)
            return;

        if (collider == null || collider.transform.parent == null)
            return;

        if (!collider.transform.parent.TryGetComponent(out Item item) || !collider.transform.parent.TryGetComponent(out Rigidbody2D rb))
            return;

        if (item.LastOwner == player)
            return;

        if (item.LastOwner.PlayerTeam.CurrentTeam != player.PlayerTeam.CurrentTeam)
            return;

        if (item.State != Item.ItemState.Dropped)
            return;

        if (Mathf.Abs(rb.linearVelocity.magnitude) < minimumCatchingVelocity)
            return;

        if (player.IsHolding)
            return;

        player.GrabItem(item,false);
    }
}