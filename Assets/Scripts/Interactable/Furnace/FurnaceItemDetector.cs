using Fusion;
using UnityEngine;

public class FurnaceItemDetector : MonoBehaviour
{
    [SerializeField] private Furnace furnace;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null)
            return;

        if (!collider.gameObject.TryGetComponent(out Item item))
            return;

        NetworkObject netObj = item.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasStateAuthority)
            return;

        if (item.ItemType != Item.Type.Clay)
            return;

        if (furnace.NetworkState != Furnace.State.Empty)
            return;

        if (!furnace.Allowed(item.LastOwner))
            return;

        furnace.PutClayIn(item.LastOwner.PlayerTeam.CurrentTeam);
        netObj.Runner.Despawn(netObj);
    }
}
