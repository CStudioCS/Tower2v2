using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private float timerLimit = 120f;
    
    [Header("References")]
    [SerializeField] private Tower towerRight;
    [SerializeField] private Tower towerLeft;
    [SerializeField] private TextMeshProUGUI timerDisplay;
    [SerializeField] private TextMeshProUGUI winnerText;
    
    public float LevelTimer { get; private set; }
    private PlayerTeam.Team winningTeam;
    
    public enum State { Lobby, Starting, Game }
    public State GameState { get; private set; } = State.Lobby;

    [SerializeField] private GameObject[] activateOnlyInLobby;
    [SerializeField] private GameObject[] activateOnlyInGame;
    [SerializeField] private Animator countdown;
    private static readonly int CountdownString = Animator.StringToHash("Countdown");

    public event Action GameAboutToStart;
    public event Action GameStarted;
    public event Action GameEnded;

    [SerializeField] private StartPoint[] startPoints;
    public StartPoint[] StartPoints => startPoints;
    private Dictionary<PlayerTeam.Team, List<StartPoint>> startPointsMap;
    public Dictionary<PlayerTeam.Team, List<StartPoint>> StartPointsMap
    {
        get
        {
            if (startPointsMap == null)
            {
                startPointsMap = new();
                foreach (StartPoint startPoint in startPoints)
                {
                    if (startPointsMap.TryGetValue(startPoint.Team, out List<StartPoint> startPoints))
                    {
                        startPoints.Add(startPoint);
                    }
                    else StartPointsMap[startPoint.Team] = new() { startPoint };
                }
            }
            return startPointsMap;
        }
    }

    public void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
        ActivateLobbyObjects(true);
        ActivateInGameObjects(false);
    }

    private void ActivateLobbyObjects(bool active = true)
    {
        foreach (GameObject go in activateOnlyInLobby)
            go.SetActive(active);
    }
    
    private void ActivateInGameObjects(bool active = true)
    {
        foreach (GameObject go in activateOnlyInGame)
            go.SetActive(active);
    }

    private void Update()
    {
        if (towerRight == null || towerLeft == null)
            return;

        if (GameState == State.Game)
        {
            LevelTimer += Time.deltaTime;
            float timeRemaining = timerLimit - LevelTimer;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerDisplay.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            if (LevelTimer >= timerLimit)
            {
                if (towerRight.Height == towerLeft.Height)
                    winningTeam = towerRight.LastPlacedTime < towerLeft.LastPlacedTime ? PlayerTeam.Team.Right : PlayerTeam.Team.Left;
                else
                    winningTeam = towerRight.Height > towerLeft.Height ? PlayerTeam.Team.Right : PlayerTeam.Team.Left;

                GameState = State.Lobby;
                EndLevel(winningTeam);
            }
        }
    }
    
    public void StartGameDelayed()
    {
        StartCoroutine(StartGameDelayedRoutine());
    }

    private IEnumerator StartGameDelayedRoutine()
    {
        GameState = State.Starting;
        ActivateLobbyObjects(false);
        countdown.SetTrigger(CountdownString);
        GameAboutToStart?.Invoke();
        
        yield return new WaitForSeconds(3); 

        GameState = State.Game;
        LevelTimer = 0;
        ActivateInGameObjects(true);
        ItemRandomizer.Reset();
        GameStarted?.Invoke();
    }

    private void EndLevel(PlayerTeam.Team winner)
    {
        GameState = State.Lobby;
        ActivateLobbyObjects(true);
        ActivateInGameObjects(false);
        Debug.Log($"Level has ended with winner {winner}");
        
        winnerText.gameObject.SetActive(true);
        winnerText.text = (winner == PlayerTeam.Team.Left ? "Left" : "Right") + " team wins!";
        GameEnded?.Invoke();
    }
}