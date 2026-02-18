using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [SerializeField] private float timerLimit = 120f;
    
    public float LevelTimer { get; private set; }
    private PlayerTeam.Team winningTeam;
    
    public enum State { Lobby, Starting, Game }
    public State GameState { get; private set; } = State.Lobby;

    //I don't really know what's the point of these lists being serializedfield-ed if you're going full linker mode, since you can't add shit through the inspector
    private List<GameObject> activateOnlyInLobby = new();
    private List<GameObject> activateOnlyInGame = new();

    private static readonly int CountdownString = Animator.StringToHash("Countdown");

    public event Action GameAboutToStart;
    public event Action GameStarted;
    public event Action GameEnded;

    private Dictionary<PlayerTeam.Team, List<StartPoint>> startPointsMap;
    public Dictionary<PlayerTeam.Team, List<StartPoint>> StartPointsMap
    {
        get
        {
            if (startPointsMap == null)
            {
                startPointsMap = new();
                if (WorldLinker.Instance.startPoints.Length <= 0)
                    Debug.LogError("Start Points haven't been defined in World Linker");

                foreach (StartPoint startPoint in WorldLinker.Instance.startPoints)
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
    }

    private void Start()
    {
        //idk if doing it this way is the best way to do it, feel free to tell me better ways
        activateOnlyInLobby.Add(CanvasLinker.Instance.MenuUI);
        activateOnlyInGame.Add(CanvasLinker.Instance.InGameUI);

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
        Tower towerRight = WorldLinker.Instance.towerRight;
        Tower towerLeft = WorldLinker.Instance.towerLeft;

        if (towerRight == null || towerLeft == null)
            return;

        if (GameState == State.Game)
        {
            LevelTimer += Time.deltaTime;
            float timeRemaining = timerLimit - LevelTimer;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            CanvasLinker.Instance.timerDisplay.text = string.Format("{0:0}:{1:00}", minutes, seconds);

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
#if DEBUG
        if (LobbyManager.Instance.DebugMode)
            StartCoroutine(StartGameDelayedRoutine(0f));
        else
#endif
            StartCoroutine(StartGameDelayedRoutine());
    }

    private IEnumerator StartGameDelayedRoutine(float delay = 3f)
    {
        GameState = State.Starting;
        ActivateLobbyObjects(false);

        if(delay > 0f)
            CanvasLinker.Instance.countdown.SetTrigger(CountdownString);

        GameAboutToStart?.Invoke();

        yield return new WaitForSeconds(delay);

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

        CanvasLinker.Instance.winnerText.gameObject.SetActive(true);
        CanvasLinker.Instance.winnerText.text = (winner == PlayerTeam.Team.Left ? "Left" : "Right") + " team wins!";
        GameEnded?.Invoke();
    }
}