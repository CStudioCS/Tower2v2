using System.Collections.Generic;
using System.Xml.Schema;
using UnityEditor.UI;
using UnityEngine;

public static class ItemRandomizer
{
    private static Item.Type[] values;

    private static Dictionary<Item.Type, float> itemWeights;
    
    private static Dictionary<Item.Type, float> initialItemWight = new ()
    {
            {Item.Type.Straw, 45f},
            {Item.Type.WoodPlank, 35f},
            {Item.Type.Brick, 20f},
    };

    private static Dictionary<Item.Type, int> itemCounter;
    private static System.Random random;
    private static readonly List<Item.Type> sequence = new();
    private static bool initialized = false;

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
        float updateRate = .3f;
        int size = sequence.Count;
        foreach(var item in values){
            itemWeights[item] = Mathf.Max(initialItemWight[item] + updateRate * (initialItemWight[item]-((float)itemCounter[item] / size)), 0);
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