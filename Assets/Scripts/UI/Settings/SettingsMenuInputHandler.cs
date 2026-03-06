using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manually drives UI navigation for the settings menu using a specific player's input actions.
/// This replaces InputSystemUIInputModule for per-player UI control in local multiplayer.
/// </summary>
public class SettingsMenuInputHandler : MonoBehaviour
{
    [Tooltip("Delay before repeated navigation when holding the stick")]
    [SerializeField] private float repeatDelay = 0.4f;
    [Tooltip("Rate of repeated navigation while holding the stick")]
    [SerializeField] private float repeatRate = 0.12f;
    [Tooltip("Stick deadzone for navigation")]
    [SerializeField] private float deadzone = 0.5f;

    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private EventSystem eventSystem;
    private float nextMoveTime;
    private Vector2 lastDirection;

    /// <summary>
    /// Binds this handler to a specific player's input actions and event system.
    /// Call this when a player opens the settings menu.
    /// </summary>
    public void Bind(PlayerInput playerInput, EventSystem targetEventSystem)
    {
        eventSystem = targetEventSystem;

        navigateAction = playerInput.actions.FindAction("UI/Navigate");
        submitAction = playerInput.actions.FindAction("UI/Submit");
        cancelAction = playerInput.actions.FindAction("UI/Cancel");

        nextMoveTime = 0f;
        lastDirection = Vector2.zero;
        enabled = true;
    }

    /// <summary>
    /// Unbinds this handler. Call when the player closes the settings menu.
    /// </summary>
    public void Unbind()
    {
        navigateAction = null;
        submitAction = null;
        cancelAction = null;
        eventSystem = null;
        enabled = false;
    }

    private void Update()
    {
        if (eventSystem == null) return;

        HandleNavigation();
        HandleSubmit();
        HandleCancel();
    }

    private void HandleNavigation()
    {
        Vector2 input = navigateAction.ReadValue<Vector2>();

        // Apply deadzone
        if (input.magnitude < deadzone)
        {
            // Reset repeat when stick returns to center
            lastDirection = Vector2.zero;
            nextMoveTime = 0f;
            return;
        }

        // Determine dominant direction (up/down/left/right)
        MoveDirection direction = GetMoveDirection(input);

        if (Time.unscaledTime < nextMoveTime)
            return;

        // Set repeat timing
        bool isNewDirection = (Vector2.Dot(input.normalized, lastDirection.normalized) < 0.5f);
        if (isNewDirection || lastDirection == Vector2.zero)
            nextMoveTime = Time.unscaledTime + repeatDelay;
        else
            nextMoveTime = Time.unscaledTime + repeatRate;

        lastDirection = input;

        // Send move event to the EventSystem
        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected == null)
        {
            // If nothing is selected, select the first object
            if (eventSystem.firstSelectedGameObject != null)
                eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
            return;
        }

        Selectable current = selected.GetComponent<Selectable>();
        if (current == null) return;

        Selectable next = direction switch
        {
            MoveDirection.Up => current.FindSelectableOnUp(),
            MoveDirection.Down => current.FindSelectableOnDown(),
            MoveDirection.Left => current.FindSelectableOnLeft(),
            MoveDirection.Right => current.FindSelectableOnRight(),
            _ => null
        };

        if (next != null)
            eventSystem.SetSelectedGameObject(next.gameObject);
    }

    private void HandleSubmit()
    {
        if (!submitAction.WasPressedThisFrame()) return;

        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected == null) return;

        ExecuteEvents.Execute(selected, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
    }

    private void HandleCancel()
    {
        if (!cancelAction.WasPressedThisFrame()) return;

        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected == null) return;

        ExecuteEvents.Execute(selected, new BaseEventData(eventSystem), ExecuteEvents.cancelHandler);
    }

    private MoveDirection GetMoveDirection(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return input.x > 0 ? MoveDirection.Right : MoveDirection.Left;
        else
            return input.y > 0 ? MoveDirection.Up : MoveDirection.Down;
    }

    private void Awake()
    {
        enabled = false;
    }
}

