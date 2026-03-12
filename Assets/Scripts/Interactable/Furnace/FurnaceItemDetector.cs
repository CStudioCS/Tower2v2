using UnityEngine;

public class FurnaceItemDetector : MonoBehaviour
{
    [SerializeField] private Furnace furnace;
    private void OnCollisionEnter2D(Collision2D collider)
    {
        if(collider != null && collider.gameObject.TryGetComponent<Item>(out Item item) && item.ItemType == Item.Type.Clay && furnace.FurnaceState == Furnace.State.Empty)
        {
            furnace.PutClayIn(item.LastOwner.PlayerTeam.CurrentTeam);
            Destroy(collider.gameObject);
        }
    }
}
