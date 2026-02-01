using System;
using System.Collections.Generic;

public static class ResourceRandomizer
{
    private static Resources.Type[] values;
    private static Random random;
    private static readonly List<Resources.Type> sequence = new();
    private static bool initialized = false;
    private static bool wasReset = true;

    private static void Initialize()
    {
        if (initialized) return;

        random = new Random();
        values = new[] { Resources.Type.Straw, Resources.Type.WoodPlank, Resources.Type.Brick };
        initialized = true;
    }

    public static Resources.Type GetAt(int index)
    {
        Initialize();

        wasReset = false;
        
        while (sequence.Count <= index)
        {
            sequence.Add(values[random.Next(values.Length)]);
        }

        return sequence[index];
    }

    public static void TryReset()
    {
        if (wasReset) return;
        Reset();
    }
    
    private static void Reset()
    {
        random = new Random();
        sequence.Clear();
        initialized = true;
        wasReset = true;
    }
}