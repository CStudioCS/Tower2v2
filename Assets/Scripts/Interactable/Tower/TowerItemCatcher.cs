using Fusion;
using UnityEngine;

public class TowerItemCatcher : MonoBehaviour
{
    [SerializeField] private Tower tower;
    private Item lastItem;
    
    [SerializeField] private float minCatchVelocity = 2f;

    private void OnTriggerEnter2D(Collider2D collider) => Trigger(collider);

    public void Trigger(Collider2D collider)
    {
        if (collider == null || !collider.gameObject.TryGetComponent(out Item item))
            return;

        NetworkObject netObj = item.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasStateAuthority) 
            return;

        if (!tower.IsItemCorrect(item.ItemType))
            return;
        
        if (item.LastOwner.PlayerTeam.CurrentTeam != tower.Team)
            return;

        if (item.State != Item.ItemState.Dropped)
            return;
        
        if (item.originallyCollectedByTeam != tower.Team)
            item.LastOwner.PlayerStats.StolenItems++;

        if (lastItem != null && lastItem == item)
            return;
        
        if (item.velocitySqrMagnitudeForTowerItemCatcher < minCatchVelocity * minCatchVelocity)
            return;
        
        lastItem = item;
        tower.ConstructPiece(item.ItemType);
        netObj.Runner.Despawn(netObj);
    }
}
