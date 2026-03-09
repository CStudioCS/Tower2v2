using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private float timerLimit = 120f;
    [SerializeField] private float secondsBeforeGameEnd = 5f; //I literally cannot name any of the shit in this PR feel free to rename
    
    public float LevelTimer { get; private set; }
    private float TimeRemaining => timerLimit - LevelTimer;
    private PlayerTeam.Team winningTeam;
    public float LobbyUIFadeTime = 1f;
    
    public enum State { Lobby, Starting, Game, EndScreen }
    public State GameState { get; private set; } = State.Lobby;

    private static readonly int CountdownString = Animator.StringToHash("Countdown");

    public event Action GameAboutToStart;
    public event Action GameStarted;
    public event Action GameEnded;
    public event Action ReturnedToLobby;

    public event Action<bool> SetActiveLobbyUI; // TODO refactor, this shouldn't be an event
    public event Action<bool> SetActiveInGameUI; // TODO refactor, this shouldn't be an event
    public event Action FewSecondsBeforeGameEnded;

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

    public static bool InGame => Instance.GameState == State.Game;

    public void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }

    private void Start()
    {
        ActivateLobbyObjects(true);
        ActivateInGameObjects(false, true);
    }

    private void ActivateLobbyObjects(bool active = true)
    {
        FadeInNOutUtility.FadeInOrOut(CanvasLinker.Instance.LobbyUI, LobbyUIFadeTime, active);
        SetActiveLobbyUI?.Invoke(active);
    }
    
    private void ActivateInGameObjects(bool active = true, bool instantaneous = false)
    {
        //The In game UI only starts as deactivated, then becomes active and never deactivates. This makes it so that
        //implementing animations for when the game ends is easy. Some extra Update loops aren't too bad performance wise
        //Also I don't think it breaks anything

        if (instantaneous || active) CanvasLinker.Instance.InGameUI.gameObject.SetActive(active);

        SetActiveInGameUI?.Invoke(active);
    }

    private void Update()
    {
        Tower towerRight = WorldLinker.Instance.towerRight;
        Tower towerLeft = WorldLinker.Instance.towerLeft;

        if (towerRight == null || towerLeft == null)
            return;

        if (GameState == State.Game)
        {
            bool alreadyXSecondsBeforeEnd = timerLimit - LevelTimer <= secondsBeforeGameEnd;

            LevelTimer += Time.deltaTime;
            float timeRemaining = TimeRemaining;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            // I'm not sure which logic is best... should it would down to 0 or to 1
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            // int seconds = Mathf.CeilToInt(timeRemaining % 60);
            // if (seconds == 60) { minutes++; seconds = 0; }
            CanvasLinker.Instance.timerDisplay.text = string.Format("{0:0}:{1:00}", minutes, seconds);

            if (timerLimit - LevelTimer <= secondsBeforeGameEnd && timerLimit - (LevelTimer - Time.deltaTime) > secondsBeforeGameEnd)
                FewSecondsBeforeGameEnded?.Invoke();

            if (LevelTimer >= timerLimit)
            {
                if (towerRight.Height == towerLeft.Height)
                    winningTeam = towerRight.LastPlacedTime < towerLeft.LastPlacedTime ? PlayerTeam.Team.Right : PlayerTeam.Team.Left;
                else
                    winningTeam = towerRight.Height > towerLeft.Height ? PlayerTeam.Team.Right : PlayerTeam.Team.Left;

                GameState = State.Lobby;
                EndLevel(winningTeam);
                CanvasLinker.Instance.timerDisplay.text = "0:00";
            }
        }
    }
    
    public void StartGameDelayed()
    {
        StartCoroutine(StartGameDelayedRoutine());
    }

    private IEnumerator StartGameDelayedRoutine(float delay = 3f)
    {
        GameState = State.Starting;
        ActivateLobbyObjects(false);

        if(delay > 0f)
            CanvasLinker.Instance.countdown.SetTrigger(CountdownString);

        ItemRandomizer.Instance.Reset();
        GameAboutToStart?.Invoke();

        yield return new WaitForSeconds(delay);

        GameState = State.Game;
        LevelTimer = 0;
        ActivateInGameObjects(true);
        GameStarted?.Invoke();
    }

    private void EndLevel(PlayerTeam.Team winner)
    {
        SoundManager.instance.PlaySound("EndLevel");

        GameState = State.EndScreen;
        ActivateInGameObjects(false);
        Debug.Log($"Level has ended with winner {winner}");

        //CanvasLinker.Instance.winnerText.gameObject.SetActive(true);
        //CanvasLinker.Instance.winnerText.text = (winner == PlayerTeam.Team.Left ? "Left" : "Right") + " team wins!";
        GameEnded?.Invoke();
    }

    public void ForceReturnToLobby()
    {
        SetGameStateToLobby();
        SetActiveInGameUI?.Invoke(false);
        SetActiveLobbyUI?.Invoke(true);
    }
    
    public void SetGameStateToLobby()
    {
        ActivateLobbyObjects(true);
        GameState = State.Lobby;

        ReturnedToLobby?.Invoke();
    }
}