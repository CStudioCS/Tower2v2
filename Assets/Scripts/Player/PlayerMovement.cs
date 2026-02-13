using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float friction = 140f;
    [SerializeField] private float gamepadDeadzone = 0.05f;
    [SerializeField] private float gamepadmaxSpeedThreashold = 0.5f;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Rigidbody2D rb;
    public Rigidbody2D Rb => rb;
    private InputAction moveAction;

    private bool gameStartinglock;

    private void Awake()
    {
        moveAction = playerInput.actions.FindAction("Gameplay/Move");
    }

    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameStarted += OnGameStarted;
    }

    private void FixedUpdate()
    {
        if (player.Interacting)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (gameStartinglock)
            return;
    
        rb.linearVelocity = VelocityApproach();
    }


    //this is adapted from code from Unnamed check it out on https://fypur.itch.io/unnamed (tsais le mec qui fait sa pub sans aucune honte)
    /// <summary>
    /// Make your character accelerate or use friction depending on player input and current speed
    /// </summary>
    private Vector2 VelocityApproach()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();

        //We wanna move and we're not at top speed -> accelerate
        if (move.sqrMagnitude > gamepadDeadzone * gamepadDeadzone && rb.linearVelocity.sqrMagnitude < maxSpeed * maxSpeed)
        {
            //Account for the fact that move can be of norm different than one (for controllers when moving slowly)
            Vector2 apporached = (move.sqrMagnitude > gamepadmaxSpeedThreashold * gamepadmaxSpeedThreashold ? move.normalized : move) * maxSpeed;
            return Approach(rb.linearVelocity, apporached, acceleration * Time.deltaTime);
        }

        //We don't wanna move or we're at max speed -> friction (friction is just reverse acceleration, it's not a multiple of velocity)
        return Approach(rb.linearVelocity, Vector2.zero, friction * Time.deltaTime);
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
        gameStartinglock = true;
    }

    private void OnGameStarted()
    {
        gameStartinglock = false;
    }
}