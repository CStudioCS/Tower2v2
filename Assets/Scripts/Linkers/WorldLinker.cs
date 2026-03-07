using UnityEngine;

public class WorldLinker : MonoBehaviour
{
    [HideInInspector] public static WorldLinker Instance;

    public Tower towerLeft;
    public Tower towerRight;

    private void Awake()
    {
        // This is not a true singleton, as the world may change and the towers will change.
        // It is true that at a given point in time, there should be only one active Instance.
        // But any new Instance overrides the previous one.
        Instance = this;
    }
}
