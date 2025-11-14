using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 5;

    private Rigidbody2D rb;
    private InputAction moveAction;
    private InputAction pickUpAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveAction = InputSystem.actions.FindAction("Move");
        pickUpAction = InputSystem.actions.FindAction("PickUp");
    }

    void Update()
    {
        rb.linearVelocity = moveAction.ReadValue<Vector2>() * speed;
    }
}
