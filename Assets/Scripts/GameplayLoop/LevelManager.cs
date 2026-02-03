using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private Tower towerRight;
    [SerializeField] private Tower towerLeft;
    [SerializeField] private TMP_Text timerDisplay;
    [SerializeField] private float timerLimit = 120f;
    [SerializeField] private TextMeshProUGUI winnerText;
    
    public float LevelTimer { get; private set; }
    private Team winningTeam;

    public enum Team { Right, Left }
    
    public enum State { Lobby, Starting, Game }
    public State GameState { get; private set; } = State.Lobby;

    [SerializeField] private GameObject[] activateOnlyInLobby;
    [SerializeField] private GameObject[] activateOnlyInGame;
    [SerializeField] private Animator countdown;
    private static readonly int CountdownString = Animator.StringToHash("Countdown");

    public event Action GameAboutToStart;
    public event Action GameStarted;
    public event Action GameEnded;

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
                if (towerRight.height == towerLeft.height)
                    winningTeam = towerRight.lastPlacedTime < towerLeft.lastPlacedTime ? Team.Right : Team.Left;
                else
                    winningTeam = towerRight.height > towerLeft.height ? Team.Right : Team.Left;

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
        ResourceRandomizer.Reset();
        GameStarted?.Invoke();
    }

    private void EndLevel(Team winner)
    {
        GameState = State.Lobby;
        ActivateLobbyObjects(true);
        ActivateInGameObjects(false);
        Debug.Log($"Level has ended with winner {winner}");
        
        winnerText.gameObject.SetActive(true);
        winnerText.text = (winner == Team.Left ? "Left" : "Right") + " team wins!";
        GameEnded?.Invoke();
    }
}