using System.Collections;
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
        StartPoint first = LevelManager.Instance.StartPoints[0];
        transform.position = first.transform.position;
    }
    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
    }
}