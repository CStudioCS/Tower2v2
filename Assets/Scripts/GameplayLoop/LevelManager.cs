using System;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : NetworkBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private float timerLimit = 120f;
    public float TimerLimit => timerLimit;
    [SerializeField] private float secondsBeforeGameEnd = 5f; //I literally cannot name any of the shit in this PR feel free to rename
    
    public float LobbyUIFadeTime = 1f;
    
    public enum State { Lobby, Starting, Game, EndScreen }

    [Networked, OnChangedRender(nameof(OnGameStateChanged))]
    private State gameState { get; set; }
    public State GameState
    {
        get {
            if (Object?.IsValid != true)
                return State.Lobby;

            return gameState;
        }
        set {
            if (Object?.IsValid == true && HasStateAuthority)
                gameState = value;
        }
    }


    [Networked] public TickTimer LevelEndTimer { get; set; }
    [Networked] public TickTimer StartDelayTimer { get; set; }
    [Networked] public PlayerTeam.Team WinningTeam { get; set; }

    public float LevelTimer
    {
        get
        {
            if (Runner == null || !LevelEndTimer.IsRunning) return 0f;
            return TimerLimit - (LevelEndTimer.RemainingTime(Runner) ?? 0f);
        }
    }

    private static readonly int CountdownString = Animator.StringToHash("Countdown");

    // Sorry I had to make them static :'(
    // I don't like it either...
    public static event Action GameAboutToStart;
    public static event Action GameStarted;
    public static event Action GameEnded;
    public static event Action ReturnedToLobby;

    public static event Action FewSecondsBeforeGameEnded;
    private bool hasInvokedFewSecondsWarning;
    private float? savedTime;

    private Dictionary<PlayerTeam.Team, List<StartPoint>> startPointsMap;
    public Dictionary<PlayerTeam.Team, List<StartPoint>> StartPointsMap
    {
        get
        {
            if (startPointsMap == null)
            {
                startPointsMap = new();
                if (StartPointLinker.Instance.startPoints.Length <= 0)
                    Debug.LogError("Start Points haven't been defined in Start Point Linker");

                foreach (StartPoint startPoint in StartPointLinker.Instance.startPoints)
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

    public static bool InGame => Instance?.Object?.IsValid == true && Instance.GameState == State.Game;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
            GameState = State.Lobby;
    }

    private void Start()
    {
        ActivateLobbyObjects(true);
        ActivateInGameObjects(false, true);
    }

    private void ActivateLobbyObjects(bool active = true)
    {
        FadeInNOutUtility.FadeInOrOut(CanvasLinker.Instance.LobbyUI, LobbyUIFadeTime, active);
    }
    
    private void ActivateInGameObjects(bool active = true, bool instantaneous = false)
    {
        //The In game UI only starts as deactivated, then becomes active and never deactivates. This makes it so that
        //implementing animations for when the game ends is easy. Some extra Update loops aren't too bad performance wise
        //Also I don't think it breaks anything

        if (instantaneous || active) CanvasLinker.Instance.InGameUI.gameObject.SetActive(active);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (GameState == State.Starting)
        {
            if (StartDelayTimer.Expired(Runner))
            {
                GameState = State.Game;
                LevelEndTimer = TickTimer.CreateFromSeconds(Runner, TimerLimit);
            }
        }
        else if (GameState == State.Game)
        {
            if (LevelEndTimer.Expired(Runner))
            {
                Tower towerRight = TowerLinker.Instance.TowerMap[PlayerTeam.Team.Right];
                Tower towerLeft = TowerLinker.Instance.TowerMap[PlayerTeam.Team.Left];

                if (towerRight == null || towerLeft == null)
                    return;

                if (towerRight.Height == towerLeft.Height)
                    WinningTeam = towerRight.LastPlacedTime < towerLeft.LastPlacedTime ? PlayerTeam.Team.Right : PlayerTeam.Team.Left;
                else
                    WinningTeam = towerRight.Height > towerLeft.Height ? PlayerTeam.Team.Right : PlayerTeam.Team.Left;

                GameState = State.EndScreen;
            }
        }
    }

    public override void Render()
    {
        if (GameState == State.Game && LevelEndTimer.IsRunning)
        {
            float timeRemaining = LevelEndTimer.RemainingTime(Runner) ?? 0f;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            // I'm not sure which logic is best... should it would down to 0 or to 1
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            // int seconds = Mathf.CeilToInt(timeRemaining % 60);
            // if (seconds == 60) { minutes++; seconds = 0; }
            CanvasLinker.Instance.timerDisplay.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            if (!hasInvokedFewSecondsWarning && timeRemaining <= secondsBeforeGameEnd)
            {
                hasInvokedFewSecondsWarning = true;
                FewSecondsBeforeGameEnded?.Invoke();
            }
        }
    }

    public void StartGameDelayed(float delay = 3f)
    {
        if (!HasStateAuthority) return;
        GameState = State.Starting;
        StartDelayTimer = TickTimer.CreateFromSeconds(Runner, delay);
    }

    private void OnGameStateChanged()
    {
        switch (GameState)
        {
            case State.Lobby:
                SetGameStateToLobby();
                break;

            case State.Starting:
                ActivateLobbyObjects(false);
                CanvasLinker.Instance.countdown.SetTrigger(CountdownString);
                ItemRandomizer.Instance.Reset();
                GameAboutToStart?.Invoke();
                Cursor.visible = false;
                break;

            case State.Game:
                ActivateInGameObjects(true);
                GameStarted?.Invoke();
                Cursor.visible = false;
                break;

            case State.EndScreen:
                EndLevel(WinningTeam);
                CanvasLinker.Instance.timerDisplay.text = "0:00";
                break;
        }
    }

    public void PauseTimer(bool isPaused)
    {
        // Only pause in single mode
        if (NetworkManager.Instance?.IsSinglePlayer != true)
            return;

        if (isPaused)
        {
            savedTime = LevelEndTimer.RemainingTime(Runner);
            LevelEndTimer = TickTimer.None;
        }
        else if (savedTime.HasValue)
        {
            LevelEndTimer = TickTimer.CreateFromSeconds(Runner, savedTime.Value);
            savedTime = null;
        }
    }

    private void EndLevel(PlayerTeam.Team winner)
    {
        SoundManager.instance.PlaySound("EndLevel");

        ActivateInGameObjects(false);
        Debug.Log($"Level has ended with winner {winner}");

        //CanvasLinker.Instance.winnerText.gameObject.SetActive(true);
        //CanvasLinker.Instance.winnerText.text = (winner == PlayerTeam.Team.Left ? "Left" : "Right") + " team wins!";
        GameEnded?.Invoke();
    }

    public void ForceReturnToLobby()
    {
        if (!HasStateAuthority)
             return;   
        
        GameState = State.Lobby;
    }
    
    public void SetGameStateToLobby()
    {
        ActivateLobbyObjects(true);
        ActivateInGameObjects(false);

        ReturnedToLobby?.Invoke();
    }
}