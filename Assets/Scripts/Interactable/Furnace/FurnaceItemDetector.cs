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

        if (item.ItemType != Item.Type.Clay)
            return;

        if (furnace.FurnaceState != Furnace.State.Empty)
            return;

        if (!furnace.Allowed(item.LastOwner))
            return;

        furnace.PutClayIn(item.LastOwner.PlayerTeam.CurrentTeam);
        Destroy(collider.gameObject);
    }
}
