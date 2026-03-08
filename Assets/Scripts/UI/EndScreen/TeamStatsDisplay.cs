using UnityEngine;

public class TeamStatsDisplay : MonoBehaviour
{
    [SerializeField] private StatDisplayer[] statDisplayers;

    public void Initialize(TeamStats stats)
    {
        for (int i = 0; i < statDisplayers.Length; i++)
        {
            StatDisplayer statDisplayer = statDisplayers[i];
            statDisplayer.Initialize(stats.stats[i]);
        }
    }
}
