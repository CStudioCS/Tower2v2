using System;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Tower TowerRight;
    [SerializeField] private Tower TowerLeft;
    [SerializeField] private TMP_Text timerDisplay;
    public float timerLimit = 120f;
    public float levelTimer;
    private Team winner;

    public enum Team { Right, Left }
    
    public enum State { Lobby, Game }
    public State GameState { get; private set; }

    public void Awake()
    {
        if(Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    void Start()
    {
        levelTimer = 0;
    }

    void Update()
    {
        if (TowerRight == null || TowerLeft == null)
            return;

        if (GameState == State.Game)
        {
            levelTimer += Time.deltaTime;
            float timeRemaining = timerLimit - levelTimer;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerDisplay.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            if(levelTimer >= timerLimit)
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

    private void EndLevel(Team winner)
    {
        Debug.Log($"Level has ended with winner {winner}");
    }
}