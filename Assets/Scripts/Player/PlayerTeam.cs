using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTeam : MonoBehaviour
{
    public enum Team { Right, Left }
    public Team CurrentTeam { get; private set; } = Team.Left;

    public event Action TeamChanged;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    public int TeamPlayerIndex { get; private set; }

    private Team CurrentPositionTeam => transform.position.x > 0 ? Team.Right : Team.Left;


    private void Start()
    {
        SetTeam(CurrentPositionTeam);
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
        Team newTeam = CurrentPositionTeam;
        if (newTeam != CurrentTeam)
        {
            if(transform.position.x > 0)
            {
                SoundManager.instance.PlaySound("ChangeTeam1");
            }
            else
            {
                SoundManager.instance.PlaySound("ChangeTeam2");
            }
            SetTeam(newTeam);
        }
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
}