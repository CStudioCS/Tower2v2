using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int itemsPlaced;
    public int woodCut;
    public int stolenItems; //Item collected by other team, but used on our tower

    public int strawCollected;
    public int woodLogsCollected;
    public int clayCollected;
    public int bricksCooked;

    private void Awake()
    {
        LevelManager.Instance.GameStarted += ResetStats;
    }

    public void OnCollectedItem(Item.Type itemType)
    {
        switch (itemType)
        {
            case Item.Type.Clay:
                clayCollected++;
                break;
            case Item.Type.Straw:
                strawCollected++;
                break;
            case Item.Type.WoodLog:
                woodLogsCollected++;
                break;
            default:
                Debug.LogError($"This type of item {itemType} is not supported in player stats !");
                break;
        }
    }

    public void ResetStats()
    {
        itemsPlaced = 0;
        woodCut = 0;
        stolenItems = 0;

        strawCollected = 0;
        woodLogsCollected = 0;
        clayCollected = 0;
        bricksCooked = 0;
    }

    private void OnDisable()
    {
        LevelManager.Instance.GameStarted -= ResetStats;
    }
}
