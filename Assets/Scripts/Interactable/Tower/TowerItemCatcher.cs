using UnityEngine;

public class TowerItemCatcher : MonoBehaviour
{
    [SerializeField] private Tower tower;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null || !collider.gameObject.TryGetComponent(out Item item))
            return;
        
        if (!tower.IsItemCorrect(item))
            return;
        
        if (item.LastOwner.PlayerTeam.CurrentTeam != tower.Team)
            return;

        if (item.State != Item.ItemState.Dropped)
            return;
        
        if (item.originallyCollectedByTeam != tower.Team)
            item.LastOwner.PlayerStats.stolenItems++;
        
        tower.ConstructPiece(item.ItemType);
        Destroy(collider.gameObject);
    }
}
