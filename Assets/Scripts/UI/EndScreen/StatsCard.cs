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

        yield return LMotion.Create((Vector2)transform.position, -dropdownOffset, dropdownTime).WithEase(Ease.OutCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();
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

            teamStatsDisplay.teamItemsStolen += players[i].PlayerStats.stolenItems;
            teamStatsDisplay.teamWoodCut += players[i].PlayerStats.woodCut;
            teamStatsDisplay.teamDistanceTravelled += players[i].PlayerStats.distanceTravelled;
            
        }

        blueStatsDisplays.UpdateText();
        redStatsDisplays.UpdateText();
    }
}
