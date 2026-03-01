using LitMotion;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatsCard : MonoBehaviour
{
    [SerializeField] private StatsDisplay[] PlayerStatsDisplay;

    [SerializeField] private float dropdownTime;
    [SerializeField] private Vector2 dropdownOffset;

    [Serializable] class StatsDisplay
    {
        public TMP_Text Stat1Text;
        public TMP_Text Stat2Text;
        public TMP_Text Stat3Text;
    }

    private void Awake()
    {
        if (PlayerStatsDisplay.Length != 4)
            Debug.LogError("Player End Screen Stats Card has Stats Text not linked up right");
    }

    public IEnumerator Dropdown()
    {
        DisplayStats();

        yield return LMotion.Create(dropdownOffset, Vector2.zero, dropdownTime).WithEase(Ease.OutCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();
    }

    private void DisplayStats()
    {
        PlayerInput[] playerInputs = GameStartManager.Instance.PlayerInputs; //dirty but idc at this point
        PlayerStats[] playerStats = new PlayerStats[playerInputs.Length];

        for (int i = 0; i < playerInputs.Length; i++)
            playerStats[i] = playerInputs[i].GetComponent<PlayerStats>();

        for (int i = 0; i < playerStats.Length; i++)
        {
            PlayerStatsDisplay[i].Stat1Text.text = "Items placed : " + playerStats[i].itemsPlaced;
            PlayerStatsDisplay[i].Stat2Text.text = "Wood cut : " + playerStats[i].woodCut;
            PlayerStatsDisplay[i].Stat3Text.text = "Items stolen : " + playerStats[i].stolenItems;
        }
    }
}
