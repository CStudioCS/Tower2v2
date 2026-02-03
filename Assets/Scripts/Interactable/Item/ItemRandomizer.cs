using System.Collections.Generic;

public static class ItemRandomizer
{
    private static Item.Type[] values;
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
            sequence.Add(values[random.Next(values.Length)]);
        }

        return sequence[index];
    }
}