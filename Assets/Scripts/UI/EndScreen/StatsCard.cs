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

    [SerializeField] private TextMeshProUGUI cardText;

    public IEnumerator Dropdown(TowerCard towerCard)
    {
        DisplayStats();

        yield return LMotion.Create(dropdownOffset, Vector2.zero, dropdownTime).WithEase(Ease.OutCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();

        towerCard.gameObject.SetActive(false);

        yield return new WaitUntil(() => Input.anyKey);

        if (NetworkManager.Instance?.IsClient == true)
        {
            cardText.text = "Waiting for host to return to lobby...";
            yield return new WaitUntil(() => LevelManager.Instance.GameState == LevelManager.State.Lobby);
        }

        yield return LMotion.Create((Vector2)transform.localPosition, dropdownOffset, dropdownTime).WithEase(Ease.InCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();
    }

    private void DisplayStats()
    {
        List<Player> players = GameStartManager.Instance.Players;

        TeamStats blueTeamStats = new TeamStats(PlayerTeam.Team.Left);
        TeamStats redTeamStats = new TeamStats(PlayerTeam.Team.Right);;
        
        for (int i = 0; i < players.Count; i++)
        {
            TeamStats teamStats = players[i].PlayerTeam.CurrentTeam == PlayerTeam.Team.Left ? blueTeamStats : redTeamStats;

            teamStats.itemsStolen.Add(players[i].PlayerStats.StolenItems);
            teamStats.woodCut.Add(players[i].PlayerStats.WoodCut);
            teamStats.distanceTravelled.Add((int) players[i].PlayerStats.DistanceTravelled);
        }

        blueStatsDisplays.Initialize(blueTeamStats);
        redStatsDisplays.Initialize(redTeamStats);
    }
}
