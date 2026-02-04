using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInitPosition : MonoBehaviour
{
    [SerializeField] private PlayerTeam playerTeam;
    [SerializeField] private StartPoint[] startPoints;
    
    private void Start()
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