using UnityEngine;

public class FurnaceItemDetector : MonoBehaviour
{
    [SerializeField] private Furnace furnace;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider != null && collider.gameObject.TryGetComponent(out Item item) && item.ItemType == Item.Type.Clay && furnace.FurnaceState == Furnace.State.Empty)
        {
            furnace.PutClayIn(item.LastOwner.PlayerTeam.CurrentTeam);
            Destroy(collider.gameObject);
        }
    }
}
