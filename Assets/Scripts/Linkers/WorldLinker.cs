using UnityEngine;

public class WorldLinker : MonoBehaviour
{
    [HideInInspector] public static WorldLinker Instance;

    public Tower towerLeft;
    public Tower towerRight;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);

        Instance = this;
    }
}
