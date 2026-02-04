using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 7f;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Rigidbody2D rb;
    private InputAction moveAction;

    private void Awake()
    {
        moveAction = playerInput.actions.FindAction("Gameplay/Move");
    }

    private void FixedUpdate()
    {
        if (player.Interacting)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        rb.linearVelocity = moveAction.ReadValue<Vector2>() * speed;
    }
}