using LitMotion;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StatsCard : MonoBehaviour
{
    [SerializeField] private float dropdownTime;
    [SerializeField] private Vector2 dropdownOffset;

    [SerializeField] private TeamStatsDisplay blueStatsDisplays;
    [SerializeField] private TeamStatsDisplay redStatsDisplays;

    public IEnumerator Dropdown(TowerCard towerCard)
    {
        DisplayStats();

        yield return LMotion.Create(dropdownOffset, Vector2.zero, dropdownTime).WithEase(Ease.OutCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();

        towerCard.gameObject.SetActive(false);

        yield return new WaitUntil(() => Input.anyKey);

        yield return LMotion.Create((Vector2)transform.localPosition, dropdownOffset, dropdownTime).WithEase(Ease.InCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();
    }

    private void DisplayStats()
    {
        List<PlayerInput> playerInputs = GameStartManager.Instance.Players; //dirty but idc at this point
        Player[] players = new Player[playerInputs.Count];

        for (int i = 0; i < playerInputs.Count; i++)
            players[i] = playerInputs[i].GetComponent<Player>();

        TeamStats blueTeamStats = new TeamStats(PlayerTeam.Team.Left);
        TeamStats redTeamStats = new TeamStats(PlayerTeam.Team.Right);;
        
        for (int i = 0; i < players.Length; i++)
        {
            TeamStats teamStats = players[i].PlayerTeam.CurrentTeam == PlayerTeam.Team.Left ? blueTeamStats : redTeamStats;

            teamStats.itemsStolen.Add(players[i].PlayerStats.stolenItems);
            teamStats.woodCut.Add(players[i].PlayerStats.woodCut);
            teamStats.distanceTravelled.Add((int) players[i].PlayerStats.distanceTravelled);
        }

        blueStatsDisplays.Initialize(blueTeamStats);
        redStatsDisplays.Initialize(redTeamStats);
    }
}
