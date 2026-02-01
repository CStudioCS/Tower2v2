using System;
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
        PlayerInput.all.Sum(playerInput => {
            PlayerTeam team = playerInput.GetComponent<PlayerTeam>();
            return team != null && team.CurrentTeam == LevelManager.Team.Right ? 1 : -1;
        });
    private bool TeamsBalanced => PlayerBalance == 0;
    
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
        LobbyManager lobby = LobbyManager.Instance;
        if (lobby == null) return;
        lobby.PlayerJoined += OnPlayerJoined;
        lobby.PlayerLeft += OnPlayerLeft;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        players.Add(playerInput);
        playerInput.GetComponent<PlayerTeam>().TeamChanged += OnPlayerTeamChanged;
        
        if (PlayerCount != 4) return;
        ChangeWaitState(TeamsBalanced ? WaitState.PlayersNotReady : WaitState.UnbalancedTeams);
    }

    private void OnPlayerTeamChanged()
    {
        if (waitState == WaitState.UnbalancedTeams)
        {
            if (TeamsBalanced)
            {
                ChangeWaitState(WaitState.PlayersNotReady);
                // TODO if ready or not
            }
        }
        else if (waitState == WaitState.PlayersNotReady)
        {
            ChangeWaitState(WaitState.UnbalancedTeams);
        }
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        players.Remove(playerInput);
        playerInput.GetComponent<PlayerTeam>().TeamChanged -= OnPlayerTeamChanged;
        TryChangeWaitState(WaitState.NotEnoughPlayers);
    }

    private void OnDestroy()
    {
        LobbyManager lobby = LobbyManager.Instance;
        if (lobby == null) return;
        lobby.PlayerJoined -= OnPlayerJoined;
        lobby.PlayerLeft -= OnPlayerLeft;
    }
}