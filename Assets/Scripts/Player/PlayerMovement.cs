using Fusion;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    public float MaxSpeed => maxSpeed;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float friction = 140f;
    [SerializeField] private float gamepadDeadzone = 0.05f;
    [SerializeField] private float gamepadMaxSpeedThreshold = 0.5f;

    [SerializeField] private float lastNonZeroInputDeadzone = .25f;
    public Vector2 LastNonZeroInput { get; private set; } = new Vector2(1f,0f);//default value to avoid errors if interactable on spawn
    private Vector2 lastSpeed;

    [Networked] public Vector2 SyncVelocity { get; set; }
    public Vector2 Velocity
    {
        get => SyncVelocity;
        set
        {
            if (rb != null)
                rb.linearVelocity = value;
            SyncVelocity = value;
        }
    }

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Rigidbody2D rb;
    public Rigidbody2D Rb => rb;

    private bool gameStartingLock;

    public bool Accelerating { get; private set; }

    private void Start()
    {
        LevelManager.GameAboutToStart += OnGameAboutToStart;
        LevelManager.GameStarted += OnGameStarted;
    }

    public override void FixedUpdateNetwork()
    {
        Vector2 inputMovement = Vector2.zero;

        if (GetInput(out PlayerNetworkInput input))
        {
            // We get the player's slot number
            int mySlot = player.InputPoller.SlotIndex;

            // Read incoming input data
            PlayerData myData = default;
            if (mySlot == 0) myData = input.Player0;
            else if (mySlot == 1) myData = input.Player1;
            else if (mySlot == 2) myData = input.Player2;
            else if (mySlot == 3) myData = input.Player3;

            inputMovement = myData.Movement;

            if (inputMovement.sqrMagnitude >= lastNonZeroInputDeadzone * lastNonZeroInputDeadzone)
                LastNonZeroInput = inputMovement.normalized;
        }

        if (gameStartingLock || player.SyncIsInteracting || player.CurrentAimingState == Player.AimingState.AimingLockedIn || player.LockedInSettingsMenu || LevelManager.Instance.GameState == LevelManager.State.EndScreen)
        {
            Velocity = Vector2.zero;
            lastSpeed = Vector2.zero;
            return;
        }

        Velocity = VelocityApproach(inputMovement);
        Accelerating = lastSpeed == Vector2.zero && Velocity != Vector2.zero;
        lastSpeed = Velocity;

        if (HasStateAuthority)
            player.PlayerStats.DistanceTravelled += Velocity.magnitude * Runner.DeltaTime;
    }


    //this is adapted from code from Unnamed check it out on https://fypur.itch.io/unnamed (tsais le mec qui fait sa pub sans aucune honte)
    /// <summary>
    /// Make your character accelerate or use friction depending on player input and current speed
    /// </summary>
    private Vector2 VelocityApproach(Vector2 inputMovement)
    {
        //We wanna move and we're not at top speed -> accelerate
        if (inputMovement.sqrMagnitude > gamepadDeadzone * gamepadDeadzone && Velocity.sqrMagnitude < maxSpeed * maxSpeed)
        {
            //Account for the fact that move can be of norm different than one (for controllers when moving slowly)
            Vector2 approached = (inputMovement.sqrMagnitude > gamepadMaxSpeedThreshold * gamepadMaxSpeedThreshold ? inputMovement.normalized : inputMovement) * maxSpeed;
            return Approach(Velocity, approached, acceleration * Runner.DeltaTime);
        }

        //We don't wanna move or we're at max speed -> friction (friction is just reverse acceleration, it's not a multiple of velocity)
        return Approach(Velocity, Vector2.zero, friction * Runner.DeltaTime);
    }

    /// <summary>
    /// Returns approached version of <paramref name="value"/> towards <paramref name="approached"/> with a step size of <paramref name="move"/>
    /// </summary>
    private Vector2 Approach(Vector2 value, Vector2 approached, float move)
    {
        Vector2 dir = (approached - value);
        float maxDisplacement = dir.magnitude;
        dir.Normalize();

        return value + dir * Mathf.Min(maxDisplacement, move);
    }

    private void OnGameAboutToStart()
    {
        gameStartingLock = true;
    }

    private void OnGameStarted()
    {
        gameStartingLock = false;
    }

    private void OnDestroy()
    {
        LevelManager.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.GameStarted -= OnGameStarted;
    }
}