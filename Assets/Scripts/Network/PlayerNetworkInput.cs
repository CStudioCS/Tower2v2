using Fusion;
using UnityEngine;

/// <summary>
/// The data structure sent from the local client to the server every network tick.
/// Contains all gameplay inputs required to simulate the player.
/// </summary>
public struct PlayerData : INetworkInput
{
    public Vector2 Movement;
    public NetworkButtons Buttons;
}

/// <summary>
/// Represents the network input data for up to four players in a multiplayer session.
/// </summary>
public struct PlayerNetworkInput : INetworkInput
{
    public PlayerData Player0;
    public PlayerData Player1;
    public PlayerData Player2;
    public PlayerData Player3;
}

/// <summary>
/// Enum defining the specific buttons tracked across the network.
/// </summary>
public enum PlayerInputButtons
{
    Interact,
    Throw
}