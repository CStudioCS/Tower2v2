using System;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Tower TowerRight;
    [SerializeField] private Tower TowerLeft;
    [SerializeField] private TMP_Text timerDisplay;
    public float timerLimit = 120f;
    public float levelTimer;
    private Team winner;
    private bool isLevelActive;
    private enum Team {Right, Left}

    public void Awake()
    {
        if(instance != null)
            Destroy(instance);

        instance = this;
    }

    void Start()
    {
        levelTimer = 0; 
        isLevelActive = true;  
    }

    void Update()
    {
        if (TowerRight == null || TowerLeft == null)
            return;

        if (isLevelActive)
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

                isLevelActive = false;
                EndLevel(winner);
            }
        }
    }

    private void EndLevel(Team winner)
    {
        Debug.Log($"Level has ended with winner {winner}");
    }
}