using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class ItemRandomizer : MonoBehaviour
{
    public static ItemRandomizer Instance;

    [SerializeField] private float updateRate = .3f;
    [SerializeField] private float strawProbability = 0.50f;
    [SerializeField] private float woodPlankProbability = 0.35f;
    [SerializeField] private float brickProbability = 0.15f;
    [SerializeField] private int contextSize = 6;
    private static Item.Type[] values;

    private static Dictionary<Item.Type, float> itemWeights;
    
    private Dictionary<Item.Type, float> initialItemWight;
    private static Dictionary<Item.Type, int> itemCounter;
    private static System.Random random;
    private static readonly List<Item.Type> sequence = new();
    private static bool initialized = false;

    public void Awake()
    {
        if(Instance != null)
            Destroy(Instance);
        
        Instance = this;
    }

    private void Initialize()
    {
        if (initialized) return;

        random = new System.Random();
        values = new[]
        {
            Item.Type.Straw,
            Item.Type.WoodPlank,
            Item.Type.Brick
        };

        initialItemWight = new () {
            {Item.Type.Straw, strawProbability},
            {Item.Type.WoodPlank, woodPlankProbability},
            {Item.Type.Brick, brickProbability},
        };

        itemWeights = new Dictionary<Item.Type, float>(initialItemWight);

        initialized = true;
    }

    public static void Reset()
    {
        initialized = false;
        sequence.Clear();
    }

    public Item.Type GetAt(int index)
    {
        Initialize();

        while (sequence.Count <= index)
        {
            Item.Type newItem = GetRandomWeightedItem(itemWeights);
            sequence.Add(newItem);
            UpdateWeights();
        }

        return sequence[index];
    }

    public void UpdateWeights()
    {
        // weight update heuristic to prevent getting the same item too often 
        itemCounter = new Dictionary<Item.Type, int> 
        {
            {Item.Type.Straw, 0},
            {Item.Type.WoodPlank, 0},
            {Item.Type.Brick, 0},
        };

        for(int i = 1; i <= Math.Min(contextSize, sequence.Count); i++){
            itemCounter[sequence[^i]]++; 
        }

        foreach(var item in values){
            Debug.Log("itemWeights :" + itemWeights[item] + " " + item.ToString());
            itemWeights[item] = Mathf.Max(initialItemWight[item] + updateRate * (initialItemWight[item] -((float)itemCounter[item] /  Math.Min(contextSize, sequence.Count))), 0);
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