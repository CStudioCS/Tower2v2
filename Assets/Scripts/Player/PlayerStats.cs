using UnityEngine;
using Fusion;

public class PlayerStats : NetworkBehaviour
{
    [Networked] public int ItemsPlaced { get; set; }
    [Networked] public int WoodCut { get; set; }
    [Networked] public int StolenItems { get; set; } //Item collected by other team, but used on our tower

    [Networked] public int StrawCollected { get; set; }
    [Networked] public int WoodLogsCollected { get; set; }
    [Networked] public int ClayCollected { get; set; }
    [Networked] public int BricksCooked { get; set; }

    [Networked] public float DistanceTravelled { get; set; }

    private bool subscribed;

    private void Start()
    {
        TrySubscribe();
    }

    public override void Spawned()
    {
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed)
            return;

        subscribed = true;
        LevelManager.GameStarted += ResetStats;
    }

    public void OnCollectedItem(Item.Type itemType)
    {
        switch (itemType)
        {
            case Item.Type.Clay:
                ClayCollected++;
                break;
            case Item.Type.Straw:
                StrawCollected++;
                break;
            case Item.Type.WoodLog:
                WoodLogsCollected++;
                break;
            default:
                Debug.LogError($"This type of item {itemType} is not supported in player stats !");
                break;
        }
    }

    public void ResetStats()
    {
        if (!HasStateAuthority)
            return;

        ItemsPlaced = 0;
        WoodCut = 0;
        StolenItems = 0;

        StrawCollected = 0;
        WoodLogsCollected = 0;
        ClayCollected = 0;
        BricksCooked = 0;

        DistanceTravelled = 0;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (subscribed)
        {
            subscribed = false;
            LevelManager.GameStarted -= ResetStats;
        }
    }
}
