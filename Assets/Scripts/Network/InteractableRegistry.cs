using System.Collections.Generic;

public static class InteractableRegistry
{
    public static Dictionary<int, Interactable> All = new Dictionary<int, Interactable>();

    public static void Register(Interactable interactable) => All[interactable.NetworkId] = interactable;
    public static void Unregister(Interactable interactable) => All.Remove(interactable.NetworkId);
}