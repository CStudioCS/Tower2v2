using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameStartManager: MonoBehaviour
{
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
                { WaitState.NotEnoughPlayers, "Waiting for players..." },
                { WaitState.UnbalancedTeams, "Unbalanced teams! Waiting for someone to switch..." },
                { WaitState.PlayersNotReady, "Waiting until everyone is ready..." },
                { WaitState.GameStarting, "Game is starting!" }
            };
            return waitingMessages;
        }
    }

    private readonly List<PlayerInput> players = new();
    private int PlayerCount => players.Count;
    // Player Balance counts +1 for right team and -1 for left team. If sum is 0, teams are balanced.
    private int PlayerBalance =>
        players.Sum(playerInput => {
            Player player = playerInput.GetComponent<Player>();
            return player.PlayerTeam.CurrentTeam == LevelManager.Team.Right ? 1 : -1;
        });
    private bool TeamsBalanced => PlayerBalance == 0;
    
    private bool AllPlayersReady =>
        players.All(playerInput => {
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

    void Start()
    {
        LobbyManager.Instance.PlayerJoined += OnPlayerJoined;
        LobbyManager.Instance.PlayerLeft += OnPlayerLeft;
        LevelManager.Instance.GameEnded += OnGameEnded;
    }

    private void OnGameEnded()
    {
        foreach (PlayerInput playerInput in players)
        {
            Player player = playerInput.GetComponent<Player>();
            player.DiscardHeldItem();
            player.PlayerTeam.LobbyUpdate();
            player.PlayerControlBadge.SetUnready();
        }
        ChangeWaitState(TeamsBalanced ? WaitState.PlayersNotReady : WaitState.UnbalancedTeams);
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        players.Add(playerInput);
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
        players.Remove(playerInput);
        Player player = playerInput.GetComponent<Player>();
        player.PlayerTeam.TeamChanged -= OnPlayerTeamChanged;
        player.PlayerControlBadge.ReadyChanged -= OnPlayerReadyChanged;
        TryChangeWaitState(WaitState.NotEnoughPlayers);
    }

    private void StartGame()
    {
        ChangeWaitState(WaitState.GameStarting);
        
        LevelManager.Instance.StartGameDelayed();
    }
    
    private void OnDestroy()
    {
        LobbyManager.Instance.PlayerJoined -= OnPlayerJoined;
        LobbyManager.Instance.PlayerLeft -= OnPlayerLeft;
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }
}