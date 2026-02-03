using System;
using UnityEngine;

public class PlayerTeam : MonoBehaviour
{
    public enum Team { Right, Left }
    public Team CurrentTeam { get; private set; } = Team.Left;
    
    public event Action TeamChanged;
    
    private Team CurrentPositionTeam => transform.position.x > 0 ? Team.Right : Team.Left; 
    
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
        if (newTeam == CurrentTeam) return;
        
        CurrentTeam = newTeam;
        TeamChanged?.Invoke();
    }
}
