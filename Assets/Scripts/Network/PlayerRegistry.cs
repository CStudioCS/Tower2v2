using System.Collections.Generic;

public static class PlayerRegistry
{
    public static List<Player> All { get; } = new();

    public static void Register(Player player)
    {
        if (!All.Contains(player)) All.Add(player);
    }

    public static void Unregister(Player player)
    {
        All.Remove(player);
    }
}