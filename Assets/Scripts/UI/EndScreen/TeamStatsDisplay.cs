using TMPro;
using UnityEngine;

public class TeamStatsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text Stat1Text;
    [SerializeField] private TMP_Text Stat2Text;
    [SerializeField] private TMP_Text Stat3Text;

    public float teamDistanceTravelled { get; set; }
    public int teamItemsStolen { get; set; }
    public int teamWoodCut { get; set; }

    public void UpdateText()
    {
        Stat1Text.text = "Distance travelled : " + Mathf.Round(teamDistanceTravelled);
        Stat3Text.text = "Items stolen : " + teamItemsStolen;
        Stat2Text.text = "Wood cut : " + teamWoodCut;
    }
}
