using System.Collections.Generic;
using UnityEngine;

public class TowerLinker : MonoBehaviour
{
    public static TowerLinker Instance { get; private set; }

    [SerializeField] private Tower towerLeft;
    [SerializeField] private Tower towerRight;

    private void Awake()
    {
        // This is not a true singleton, as the world may change and the towers will change.
        // It is true that at a given point in time, there should be only one active Instance.
        // But any new Instance overrides the previous one.
        Instance = this;
    }
    
    private Dictionary<PlayerTeam.Team, Tower> towerMap;
    public Dictionary<PlayerTeam.Team, Tower> TowerMap
    {
        get
        {
            towerMap ??= new Dictionary<PlayerTeam.Team, Tower>
            {
                [PlayerTeam.Team.Left] = towerLeft,
                [PlayerTeam.Team.Right] = towerRight
            };
            return towerMap;
        }
    }
}
