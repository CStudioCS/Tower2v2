using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeam : MonoBehaviour
{
    public enum Team { Right, Left }
    public Team CurrentTeam { get; private set; } = Team.Left;
    
    public event Action TeamChanged;
    
    
    [Header("Colors")]
    [SerializeField] private Color leftTeamColor;
    [SerializeField] private Color rightTeamColor;
    
    private Dictionary<Team, Color> teamColors;
    public Dictionary<Team, Color> TeamColors
    {
        get
        {
            teamColors ??= new Dictionary<Team, Color>
            {
                { Team.Left, leftTeamColor },
                { Team.Right, rightTeamColor }
            };
            return teamColors;
        }
    }
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        UpdateColor();
    }
    
    private void Update()
    {
        switch (LevelManager.Instance?.GameState)
        {
            case LevelManager.State.Lobby:
                LobbyUpdate();
                break;
        }
    }

    public void LobbyUpdate()
    {
        Team newTeam = GetCurrentPositionTeam();
        if (newTeam != CurrentTeam)
            SetTeam(newTeam);
    }

    private Team GetCurrentPositionTeam() => transform.position.x > 0 ? Team.Right : Team.Left; 

    public void SetTeam(Team team)
    {
        CurrentTeam = team;
        UpdateColor();
        TeamChanged?.Invoke();
    }
    
    private void UpdateColor() => spriteRenderer.color = TeamColors[CurrentTeam];
}
