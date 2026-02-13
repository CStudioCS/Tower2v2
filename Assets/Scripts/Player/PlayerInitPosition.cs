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
        List<StartPoint> startPoints = LevelManager.Instance.StartPointsMap[playerTeam.CurrentTeam];
        StartPoint startPoint = startPoints[playerTeam.TeamPlayerIndex];
        transform.position = startPoint.transform.position;
    }

    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
    }
}