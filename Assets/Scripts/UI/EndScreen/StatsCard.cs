using LitMotion;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatsCard : MonoBehaviour
{
    [SerializeField] private float dropdownTime;
    [SerializeField] private Vector2 dropdownOffset;

    [SerializeField] private TeamStatsDisplay blueStatsDisplays;
    [SerializeField] private TeamStatsDisplay redStatsDisplays;

    public IEnumerator Dropdown()
    {
        DisplayStats();

        yield return LMotion.Create(dropdownOffset, Vector2.zero, dropdownTime).WithEase(Ease.OutCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();

        yield return new WaitUntil(() => Input.anyKey);
    }

    private void DisplayStats()
    {
        PlayerInput[] playerInputs = GameStartManager.Instance.PlayerInputs; //dirty but idc at this point
        Player[] players = new Player[playerInputs.Length];

        for (int i = 0; i < playerInputs.Length; i++)
            players[i] = playerInputs[i].GetComponent<Player>();

        for (int i = 0; i < players.Length; i++)
        {
            TeamStatsDisplay teamStatsDisplay = players[i].PlayerTeam.CurrentTeam == PlayerTeam.Team.Left ? blueStatsDisplays : redStatsDisplays;

            teamStatsDisplay.Stat1Text.text = "Items placed : " + players[i].PlayerStats.itemsPlaced;
            teamStatsDisplay.Stat2Text.text = "Wood cut : " + players[i].PlayerStats.woodCut;
            teamStatsDisplay.Stat3Text.text = "Items stolen : " + players[i].PlayerStats.stolenItems;
            
        }
    }
}
