using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Tower TowerRight;
    [SerializeField] private Tower TowerLeft;
    public float timerLimit = 120f;
    public float levelTimer;
    private Team winner;
    private bool isLevelActive;
    private enum Team {Right, Left}

    public void Awake()
    {
        if(instance != null)
        {
            Destroy(instance);
        }

        instance = this;
    }

    void Start()
    {
        levelTimer = 0; 
        isLevelActive = true;  
    }

    // Update is called once per frame
    void Update()
    {
        if (TowerRight == null || TowerLeft == null) return;

        if (isLevelActive)
        {
            levelTimer += Time.deltaTime;
            if(levelTimer >= timerLimit)
            {
                if (TowerRight.height == TowerLeft.height)
                {
                    winner = (TowerRight.lastPlacedTime < TowerLeft.lastPlacedTime)? Team.Right : Team.Left;
                }
                else
                { 
                    winner = (TowerRight.height > TowerLeft.height)? Team.Right : Team.Left;
                }
                isLevelActive = false;
                Debug.Log(winner);
            }
        }
    }
}