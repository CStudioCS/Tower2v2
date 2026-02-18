using System.Collections.Generic;
using UnityEngine;

public class WorldLinker : MonoBehaviour
{
    [HideInInspector] public static WorldLinker Instance;

    public Tower towerLeft;
    public Tower towerRight;

    public StartPoint[] startPoints;

    private void Awake()
    {
        if(Instance != null)
            Destroy(Instance);

        Instance = this;
    }
}
