using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public class ItemRandomizer : MonoBehaviour
{

    [SerializeField]
    private float updateRate = .3f;
    [SerializeField]
    private static float strawProbability = 0.45f;
    [SerializeField]
    private static float woodPlankProbability = 0.35f;
    [SerializeField]
    private static float brickProbability = 0.20f;

    public static ItemRandomizer Instance;
    private static Item.Type[] values;

    private static Dictionary<Item.Type, float> itemWeights;
    
    private static Dictionary<Item.Type, float> initialItemWight = new ()
    {
            {Item.Type.Straw, strawProbability},
            {Item.Type.WoodPlank, woodPlankProbability},
            {Item.Type.Brick, brickProbability},
    };

    private static Dictionary<Item.Type, int> itemCounter;
    private static System.Random random;
    private static readonly List<Item.Type> sequence = new();
    private static bool initialized = false;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        
        Instance = this;
    }
    private static void Initialize()
    {
        if (initialized) return;

        random = new System.Random();
        values = new[]
        {
            Item.Type.Straw,
            Item.Type.WoodPlank,
            Item.Type.Brick
        };
        
        itemWeights = new Dictionary<Item.Type, float>(initialItemWight);

        itemCounter = new Dictionary<Item.Type, int> 
        {
            {Item.Type.Straw, 0},
            {Item.Type.WoodPlank, 0},
            {Item.Type.Brick, 0},
        };

        initialized = true;
    }

    public static void Reset()
    {
        initialized = false;
        sequence.Clear();
    }

    public static Item.Type GetAt(int index)
    {
        Initialize();

        while (sequence.Count <= index)
        {
            Item.Type newItem = GetRandomWeightedItem(itemWeights);
            itemCounter[newItem]++;
            sequence.Add(newItem);
            UpdateWeights();
        }

        return sequence[index];
    }

    public static void UpdateWeights()
    {
        // weight update heuristic to prevent getting the same item too often 
        int size = sequence.Count;
        foreach(var item in values){
            itemWeights[item] = Mathf.Max(initialItemWight[item] + updateRate * (initialItemWight[item] -((float)itemCounter[item] / size)), 0);
        }
    }

    public static Item.Type GetRandomWeightedItem(Dictionary<Item.Type, float> ItemWeights)
    {
        float totalWeight = 0;
        foreach (var kvp in ItemWeights){
            totalWeight += kvp.Value;
        }

        float roll = (float)random.NextDouble() * totalWeight;
        float sum = 0;
        foreach (var kvp in ItemWeights){
            sum += kvp.Value;
            if (roll < sum){
                return kvp.Key;
            } 
        }

        return Item.Type.Straw;
    }
}