using System;
using UnityEngine;

public class PlayerTeam : MonoBehaviour
{
    public LevelManager.Team CurrentTeam { get; private set; } = LevelManager.Team.Left;
    
    public event Action TeamChanged;
    
    private LevelManager.Team CurrentPositionTeam => transform.position.x > 0 ? LevelManager.Team.Right : LevelManager.Team.Left; 
    
    private void Update()
    {
        switch (LevelManager.Instance?.GameState)
        {
            case LevelManager.State.Lobby:
                LobbyUpdate();
                break;
            case LevelManager.State.Game:
                GameUpdate();
                break;
        }
    }

    public void LobbyUpdate()
    {
        LevelManager.Team newTeam = CurrentPositionTeam;
        if (newTeam == CurrentTeam) return;
        
        CurrentTeam = newTeam;
        TeamChanged?.Invoke();
    }

    private void GameUpdate()
    {
        
    }
}
