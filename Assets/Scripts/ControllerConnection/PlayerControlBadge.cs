using Fusion;
using System;
using UnityEngine;

public class PlayerControlBadge : NetworkBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerTeam playerTeam;

    // Event for the UI to update the ready status (the old PlayerControlBadge)
    public event Action<bool> ReadyChanged;
    // Event for the server to check if all players are ready (the new PlayerControlBadge)
    public event Action<bool> ServerReadyChanged;

    [Networked, OnChangedRender(nameof(OnReadyChanged))]
    public NetworkBool IsReady { get; private set; }

    public override void Spawned()
    {
        if (playerTeam != null)
            playerTeam.TeamChanged += OnTeamChanged;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (playerTeam != null)
            playerTeam.TeamChanged -= OnTeamChanged;
    }

    private void OnTeamChanged()
    {
        SetUnready();
    }

    /// <summary>
    /// Called by Fusion when IsReady changes on each client.
    /// </summary>
    private void OnReadyChanged()
    {
        ReadyChanged?.Invoke(IsReady);
    }

    /// <summary>
    /// Called by the Player (which handles input) when the player wants to change their status.
    /// </summary>
    public void ToggleReady()
    {
        if (HasStateAuthority)
            IsReady = !IsReady;

        if (Runner.IsForward)
            ServerReadyChanged?.Invoke(IsReady);
    }

    public void SetUnready()
    {
        if (HasStateAuthority && IsReady)
        {
            IsReady = false;

            if (Runner.IsForward)
                ServerReadyChanged?.Invoke(false);
        }
    }
}