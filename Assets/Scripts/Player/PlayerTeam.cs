using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTeam : NetworkBehaviour
{
    public enum Team { Right, Left }

    //It syncs across the network and calls OnTeamChanged when it changes.
    [Networked, OnChangedRender(nameof(OnTeamChanged))]
    public Team CurrentTeam { get; private set; } = Team.Left;

    public event Action TeamChanged;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int TeamPlayerIndex { get; private set; }

    private Team CurrentPositionTeam => transform.position.x > 0 ? Team.Right : Team.Left;

    public static readonly Color LeftTeamColor = new Color32(92, 164, 218, 255);
    public static readonly Color RightTeamColor = new Color32(217, 69, 58, 255);
    private static Dictionary<Team, Color> s_teamColors;
    public static Dictionary<Team, Color> TeamColors => s_teamColors ??= new Dictionary<Team, Color>
    {
        { Team.Left, LeftTeamColor },
        { Team.Right, RightTeamColor }
    };

    public void InitializeTeam()
    {
        if (HasStateAuthority)
            CurrentTeam = CurrentPositionTeam;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && LevelManager.Instance?.GameState == LevelManager.State.Lobby)
            LobbyUpdate();
    }

    public void LobbyUpdate()
    {
        Team newTeam = CurrentPositionTeam;

        if (newTeam != CurrentTeam)
            CurrentTeam = newTeam;
    }

    private void OnTeamChanged()
    {
        if (CurrentTeam == Team.Right)
            SoundManager.instance.PlaySound("ChangeTeam1");
        else
            SoundManager.instance.PlaySound("ChangeTeam2");

        TeamChanged?.Invoke(); // I Kept the old event for the local UI
    }

    public void SetTeam(Team team)
    {
        CurrentTeam = team;
        TeamChanged?.Invoke();
    }

    public void InitTeamPlayerIndex(int teamPlayerIndex)
    {
        TeamPlayerIndex = teamPlayerIndex;
    }

    public Player GetTeamMate()
    {
        foreach (Player player in GameStartManager.Instance.Players)
        {
            if (player.PlayerTeam.CurrentTeam == CurrentTeam && player.PlayerTeam.TeamPlayerIndex != TeamPlayerIndex)
                return player;
        }
        Debug.LogError($"Could not find team mate for player in team {CurrentTeam} with team player index {TeamPlayerIndex}.");
        return null;
    }
}