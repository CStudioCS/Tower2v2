using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInitPosition : MonoBehaviour
{
    [SerializeField] private PlayerTeam playerTeam;
    
    public void Initialize()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
    }

    private void OnGameAboutToStart()
    {
        if (playerTeam.TeamPlayerIndex == 1)
            return;

        List<StartPoint> startPoints = LevelManager.Instance.StartPointsMap[playerTeam.CurrentTeam];
        bool isFirstStartPointUpper = startPoints[0].transform.position.y >= startPoints[1].transform.position.y;

        Player teamMate = playerTeam.GetTeamMate();
        bool chooseUpperSpawnPoint = transform.position.y >= teamMate.transform.position.y;

        int teamMateStartPointIndex = chooseUpperSpawnPoint == isFirstStartPointUpper ? 1 : 0;
        GoToStartPoint(startPoints[1 - teamMateStartPointIndex]);
        teamMate.PlayerInitPosition.GoToStartPoint(startPoints[teamMateStartPointIndex]);
    }

    private void GoToStartPoint(StartPoint startPoint) => transform.position = startPoint.transform.position;
    
    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
    }
}