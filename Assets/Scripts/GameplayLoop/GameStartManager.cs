using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameStartManager : MonoBehaviour
{
    public static GameStartManager Instance; 
    public enum WaitState
    {
        NotEnoughPlayers,
        UnbalancedTeams,
        PlayersNotReady,
        GameStarting
    }
    private WaitState waitState = WaitState.NotEnoughPlayers;
    
    private Dictionary<WaitState, string> waitingMessages;
    private Dictionary<WaitState, string> WaitingMessages
    {
        get
        {
            waitingMessages ??= new Dictionary<WaitState, string>
            {
                { WaitState.NotEnoughPlayers, "Press a key to join. Waiting for players..." },
                { WaitState.UnbalancedTeams, "Unbalanced teams! Waiting for someone to switch..." },
                { WaitState.PlayersNotReady, "Waiting until everyone is ready..." },
                { WaitState.GameStarting, "Game is starting!" }
            };
            return waitingMessages;
        }
    }

    public readonly List<PlayerInput> Players = new();
    private int PlayerCount => Players.Count;
    // Player Balance counts +1 for right team and -1 for left team. If sum is 0, teams are balanced.
    private int PlayerBalance =>
        Players.Sum(playerInput => {
            Player player = playerInput.GetComponent<Player>();
            return player.PlayerTeam.CurrentTeam == PlayerTeam.Team.Right ? 1 : -1;
        });
    private bool TeamsBalanced => PlayerBalance == 0;
    
    private bool AllPlayersReady =>
        Players.All(playerInput => {
            Player player = playerInput.GetComponent<Player>();
            return player.PlayerControlBadge.IsReady;
        });
    
    [SerializeField] private TextMeshProUGUI waitingText;
    
    private void UpdateWaitingText() => waitingText.text = WaitingMessages[waitState];
    
    private void TryChangeWaitState(WaitState newWaitState)
    {
        if (waitState == newWaitState) return;
        ChangeWaitState(newWaitState);
    }
    private void ChangeWaitState(WaitState newWaitState)
    {
        waitState = newWaitState;
        UpdateWaitingText();
    }

    public void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
            
        Instance = this;
    }

    void Start()
    {
        LobbyManager.Instance.PlayerJoined += OnPlayerJoined;
        LobbyManager.Instance.PlayerLeft += OnPlayerLeft;
        LevelManager.Instance.GameEnded += OnGameEnded;
        ChangeWaitState(WaitState.NotEnoughPlayers);
    }

    private void OnGameEnded()
    {
        foreach (PlayerInput playerInput in Players)
        {
            Player player = playerInput.GetComponent<Player>();
            player.ConsumeCurrentItem();
            player.PlayerTeam.LobbyUpdate();
            player.PlayerControlBadge.SetUnready();
        }
        ChangeWaitState(TeamsBalanced ? WaitState.PlayersNotReady : WaitState.UnbalancedTeams);
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        Players.Add(playerInput);
        Player player = playerInput.GetComponent<Player>();
        player.PlayerTeam.TeamChanged += OnPlayerTeamChanged;
        player.PlayerControlBadge.ReadyChanged += OnPlayerReadyChanged;
        
        if (PlayerCount != 4) return;
        ChangeWaitState(TeamsBalanced ? WaitState.PlayersNotReady : WaitState.UnbalancedTeams);
    }
    
    private void OnPlayerTeamChanged()
    {
        if (waitState == WaitState.UnbalancedTeams)
        {
            if (TeamsBalanced)
            {
                if (AllPlayersReady)
                    StartGame();
                else
                    ChangeWaitState(WaitState.PlayersNotReady);
            }
        }
        else if (waitState == WaitState.PlayersNotReady)
        {
            ChangeWaitState(WaitState.UnbalancedTeams);
        }
    }

    private void OnPlayerReadyChanged()
    {
        if (waitState == WaitState.PlayersNotReady)
        {
            if (AllPlayersReady)
                StartGame();
        }
    }
    
    private void OnPlayerLeft(PlayerInput playerInput)
    {
        Players.Remove(playerInput);
        Player player = playerInput.GetComponent<Player>();
        player.PlayerTeam.TeamChanged -= OnPlayerTeamChanged;
        player.PlayerControlBadge.ReadyChanged -= OnPlayerReadyChanged;
        TryChangeWaitState(WaitState.NotEnoughPlayers);
    }

    private void StartGame()
    {
        ChangeWaitState(WaitState.GameStarting);
        InitializeTeamPlayerIndices();
        LevelManager.Instance.StartGameDelayed();
    }
    
    private void OnDisable()
    {
        LobbyManager.Instance.PlayerJoined -= OnPlayerJoined;
        LobbyManager.Instance.PlayerLeft -= OnPlayerLeft;
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }

    private void InitializeTeamPlayerIndices()
    {
        int leftTeamCounter = 0;
        int rightTeamCounter = 0;
        foreach (PlayerInput playerInput in Players)
        {
            Player player = playerInput.GetComponent<Player>();
            PlayerTeam playerTeam = player.PlayerTeam;
            PlayerTeam.Team currentTeam = playerTeam.CurrentTeam;
            switch (currentTeam)
            {
                case PlayerTeam.Team.Left:
                    playerTeam.InitTeamPlayerIndex(leftTeamCounter);
                    leftTeamCounter++;
                    break;
                default:
                    playerTeam.InitTeamPlayerIndex(rightTeamCounter);
                    rightTeamCounter++;
                    break;
            }
        }
    }
}