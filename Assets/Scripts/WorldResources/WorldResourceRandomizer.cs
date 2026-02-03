using System.Collections.Generic;

public static class WorldResourceRandomizer
{
    private static WorldResources.Type[] values;
    private static System.Random random;
    private static readonly List<WorldResources.Type> sequence = new();
    private static bool initialized = false;

    private static void Initialize()
    {
        if (initialized) return;

        random = new System.Random();
        values = new[]
        {
            WorldResources.Type.Straw,
            WorldResources.Type.WoodPlank,
            WorldResources.Type.Brick
        };

        initialized = true;
    }

    public static void Reset()
    {
        initialized = false;
        sequence.Clear();
    }

    public static WorldResources.Type GetAt(int index)
    {
        Initialize();

        while (sequence.Count <= index)
        {
            sequence.Add(values[random.Next(values.Length)]);
        }

        return sequence[index];
    }
}