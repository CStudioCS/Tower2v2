using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using UnityEngine.InputSystem;

public class GameStartManager : NetworkBehaviour
{
    public static GameStartManager Instance;
    public enum WaitState
    {
        Logo,
        NotEnoughPlayers,
        UnbalancedTeams,
        PlayersNotReady,
        GameStarting
    }

    [Networked, OnChangedRender(nameof(OnWaitStateChanged))]
    public WaitState waitState { get; set; }

    private Dictionary<WaitState, string> waitingMessages;
    private Dictionary<WaitState, string> WaitingMessages
    {
        get
        {
            waitingMessages ??= new Dictionary<WaitState, string>
            {
                { WaitState.Logo, "Press any button..." },
                { WaitState.NotEnoughPlayers, "Press a key to join. Waiting for players..." },
                { WaitState.UnbalancedTeams, "Unbalanced teams! Waiting for someone to switch..." },
                { WaitState.PlayersNotReady, "Waiting until everyone is ready..." },
                { WaitState.GameStarting, "Waiting until everyone is ready..." }
                //since the text is fading out, there is not point in changing it when the game starts, else it just looks bad
            };
            return waitingMessages;
        }
    }

    private readonly List<Player> players = new();
    public List<Player> Players => players;

    public int PlayerCount => players.Count;
    // Player Balance counts +1 for right team and -1 for left team. If sum is 0, teams are balanced.
    public int PlayerBalance => players.Sum(
        player => player.PlayerTeam.CurrentTeam == PlayerTeam.Team.Right ? 1 : -1);
    private bool TeamsBalanced => PlayerBalance == 0;

    private bool AllPlayersReady => players.Count == 4 && players.All(player => player.PlayerControlBadge.IsReady);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnWaitStateChanged() => UpdateWaitingText();
    private void UpdateWaitingText() => CanvasLinker.Instance.waitingText.text = WaitingMessages[waitState];
    
    private void TryChangeWaitState(WaitState newWaitState)
    {
        if (!HasStateAuthority) return;
        if (waitState == newWaitState) return;

        waitState = newWaitState;
    }

    public override void Spawned()
    {
        LevelManager.ServerResetRequested += ResetPlayers;
        Player.PlayerSpawned += AddPlayer;
        Player.PlayerDespawned += RemovePlayer;

        if (HasStateAuthority)
            waitState = WaitState.Logo;
    }

    private void ResetPlayers()
    {
        if (!HasStateAuthority) return;

        foreach (Player player in players)
        {
            player.ConsumeCurrentItem();
            player.PlayerTeam.LobbyUpdate();
            player.PlayerControlBadge.SetUnready();
        }
        TryChangeWaitState(TeamsBalanced ? WaitState.PlayersNotReady : WaitState.UnbalancedTeams);
    }

    public void AddPlayer(Player player)
    {
        if (!players.Contains(player))
            players.Add(player);

        SoundManager.instance.PlaySound("PlayerConnect");

        player.PlayerTeam.InitializeTeam();
        player.PlayerTeam.ServerTeamChanged += OnPlayerTeamChanged;
        player.PlayerControlBadge.ServerReadyChanged += OnPlayerReadyChanged;

        if (!HasStateAuthority) return;

        if (PlayerCount != 4)
            return;

#if DEBUG
        if (TeamsBalanced && AllPlayersReady) //only ever true in debug mode where the frame the player joins it changes its team
            StartGame();
        else
#endif
        TryChangeWaitState(TeamsBalanced ? WaitState.PlayersNotReady : WaitState.UnbalancedTeams);
    }
    
    private void OnPlayerTeamChanged()
    {
        if (!HasStateAuthority) return;

        if (waitState == WaitState.UnbalancedTeams)
        {
            if (TeamsBalanced)
            {
                if (AllPlayersReady)
                    StartGame();
                else
                    TryChangeWaitState(WaitState.PlayersNotReady);
            }
        }
        else if (waitState == WaitState.PlayersNotReady)
        {
            if (!TeamsBalanced)
                TryChangeWaitState(WaitState.UnbalancedTeams);
        }
    }

    private void OnPlayerReadyChanged(bool isReady)
    {
        if (HasStateAuthority && waitState == WaitState.PlayersNotReady && AllPlayersReady)
            StartGame();
    }

    public void RemovePlayer(Player player)
    {
        players.Remove(player);
        player.PlayerTeam.ServerTeamChanged -= OnPlayerTeamChanged;
        player.PlayerControlBadge.ServerReadyChanged -= OnPlayerReadyChanged;

        if (HasStateAuthority)
            TryChangeWaitState(WaitState.NotEnoughPlayers);
    }

    private void StartGame()
    {
        if (!HasStateAuthority) return;

        TryChangeWaitState(WaitState.GameStarting);
        InitializeTeamPlayerIndices();
        LevelManager.Instance.StartGameDelayed();
        SoundManager.instance.PlaySound("Countdown");
    }

    private void Update()
    {
        if (!HasStateAuthority) return;

        if (waitState != WaitState.Logo)
            return;

        if (InputUtility.AnyInputPressed)
            TryChangeWaitState(WaitState.NotEnoughPlayers);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        LevelManager.ServerResetRequested -= ResetPlayers;
        Player.PlayerSpawned -= AddPlayer;
        Player.PlayerDespawned -= RemovePlayer;
    }

    private void InitializeTeamPlayerIndices()
    {
        int leftTeamCounter = 0;
        int rightTeamCounter = 0;
        foreach (Player player in players)
        {
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