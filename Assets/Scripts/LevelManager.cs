using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Tower TowerRight;
    [SerializeField] private Tower TowerLeft;
    [SerializeField] private TMP_Text timerDisplay;
    [SerializeField] private float timerLimit = 120f;
    public float LevelTimer { get; private set; } = 0;
    private Team winner;

    public enum Team { Right, Left }
    
    public enum State { Lobby, Starting, Game }
    public State GameState { get; private set; } = State.Lobby;

    [SerializeField] private GameObject[] activateOnlyInLobby;
    [SerializeField] private GameObject[] activateOnlyInGame;

    public event Action GameStarted;

    public void Awake()
    {
        if(Instance != null)
            Destroy(Instance);

        Instance = this;
        ActivateLobbyObjects(true);
        ActivateInGameObjects(false);
    }

    void ActivateLobbyObjects(bool on = true)
    {
        foreach (GameObject go in activateOnlyInLobby)
            go.SetActive(on);
    }
    
    void ActivateInGameObjects(bool on = true)
    {
        foreach (GameObject go in activateOnlyInGame)
            go.SetActive(on);
    }

    void Update()
    {
        if (TowerRight == null || TowerLeft == null)
            return;

        if (GameState == State.Game)
        {
            LevelTimer += Time.deltaTime;
            float timeRemaining = timerLimit - LevelTimer;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerDisplay.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            if(LevelTimer >= timerLimit)
            {
                if (TowerRight.height == TowerLeft.height)
                    winner = (TowerRight.lastPlacedTime < TowerLeft.lastPlacedTime)? Team.Right : Team.Left;
                else
                    winner = (TowerRight.height > TowerLeft.height)? Team.Right : Team.Left;

                GameState = State.Lobby;
                EndLevel(winner);
            }
        }
    }
    
    public void StartGameDelayed()
    {
        StartCoroutine(StartGameDelayedRoutine());
    }

    public IEnumerator StartGameDelayedRoutine()
    {
        GameState = State.Starting;
        ActivateLobbyObjects(false);
        yield return new WaitForSeconds(3); 
        GameState = State.Game;
        GameStarted?.Invoke();
        LevelTimer = 0;
        ActivateInGameObjects(true);
    }

    private void EndLevel(Team winner)
    {
        GameState = State.Lobby;
        ActivateLobbyObjects(true);
        ActivateInGameObjects(false);
        Debug.Log($"Level has ended with winner {winner}");
    }
}