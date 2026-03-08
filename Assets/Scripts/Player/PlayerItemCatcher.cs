using UnityEngine;

public class PlayerItemCatcher : MonoBehaviour
{
    [SerializeField] private PlayerTeam playerTeam;
    [SerializeField] private Player player;
    [SerializeField] private float minimumCatchingVelocity = 0.5f;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null || collider.transform.parent == null)
            return;

        if(!collider.transform.parent.TryGetComponent<Item>(out Item item) || !collider.transform.parent.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            return;

        if (item.LastOwner.PlayerTeam == playerTeam)
            return;

        if (item.LastOwner.PlayerTeam.CurrentTeam != playerTeam.CurrentTeam)
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