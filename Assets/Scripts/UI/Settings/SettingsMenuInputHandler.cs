using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manually drives UI navigation for the settings menu using a specific player's input actions.
/// This replaces InputSystemUIInputModule for per-player UI control in local multiplayer.
/// Navigation is index-based using an ordered list of selectables from SettingsMenu.
/// When a Slider is selected, left/right adjusts its value instead of navigating away.
/// </summary>
public class SettingsMenuInputHandler : MonoBehaviour
{
    [Tooltip("Delay before repeated navigation when holding the stick")]
    [SerializeField] private float repeatDelay = 0.4f;
    [Tooltip("Rate of repeated navigation while holding the stick")]
    [SerializeField] private float repeatRate = 0.12f;
    [Tooltip("Stick deadzone for navigation")]
    [SerializeField] private float deadzone = 0.5f;
    [Tooltip("How fast the slider moves per second when holding the stick")]
    [SerializeField] private float sliderSpeed = 1f;

    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private EventSystem eventSystem;
    private SettingsMenu settingsMenu;
    private Selectable[] selectables;
    private int currentIndex;
    private float nextMoveTime;
    private Vector2 lastDirection;

    /// <summary>
    /// Binds this handler to a specific player's input actions and event system.
    /// </summary>
    public void Bind(PlayerInput playerInput, EventSystem targetEventSystem, SettingsMenu menu)
    {
        eventSystem = targetEventSystem;
        settingsMenu = menu;
        selectables = menu.Selectables;

        navigateAction = playerInput.actions.FindAction("UI/Navigate");
        submitAction = playerInput.actions.FindAction("UI/Submit");
        cancelAction = playerInput.actions.FindAction("UI/Cancel");

        nextMoveTime = 0f;
        lastDirection = Vector2.zero;

        // Select the default selectable (e.g. Credits button)
        currentIndex = Mathf.Clamp(menu.DefaultSelectedIndex, 0, selectables.Length - 1);
        if (selectables != null && selectables.Length > 0)
            SelectCurrent();

        enabled = true;
    }

    /// <summary>
    /// Unbinds this handler.
    /// </summary>
    public void Unbind()
    {
        navigateAction = null;
        submitAction = null;
        cancelAction = null;
        eventSystem = null;
        settingsMenu = null;
        selectables = null;
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

        if (input.magnitude < deadzone)
        {
            lastDirection = Vector2.zero;
            nextMoveTime = 0f;
            return;
        }

        // If on a slider and pushing left/right, adjust the slider value continuously
        Slider activeSlider = GetSelectedSlider();
        if (activeSlider != null && Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            float range = activeSlider.maxValue - activeSlider.minValue;
            activeSlider.value += input.x * sliderSpeed * range * Time.unscaledDeltaTime;
            return;
        }

        // Discrete navigation (up/down moves through the list)
        if (Time.unscaledTime < nextMoveTime)
            return;

        int vertical = 0;
        if (Mathf.Abs(input.y) >= Mathf.Abs(input.x))
            vertical = input.y > 0 ? -1 : 1; // up = previous index, down = next index

        if (vertical == 0) return;

        bool isNewDirection = lastDirection == Vector2.zero || Vector2.Dot(input.normalized, lastDirection.normalized) < 0.5f;
        nextMoveTime = Time.unscaledTime + (isNewDirection ? repeatDelay : repeatRate);
        lastDirection = input;

        // Move index with wrapping
        int newIndex = currentIndex + vertical;
        if (newIndex < 0) newIndex = selectables.Length - 1;
        if (newIndex >= selectables.Length) newIndex = 0;

        currentIndex = newIndex;
        SelectCurrent();
    }

    private void HandleSubmit()
    {
        if (submitAction == null || !submitAction.WasPressedThisFrame()) return;

        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected == null) return;

        ExecuteEvents.Execute(selected, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
    }

    private void HandleCancel()
    {
        if (cancelAction == null || !cancelAction.WasPressedThisFrame()) return;

        settingsMenu.Close();
    }

    private void SelectCurrent()
    {
        if (selectables == null || selectables.Length == 0) return;
        eventSystem.SetSelectedGameObject(selectables[currentIndex].gameObject);
    }

    private Slider GetSelectedSlider()
    {
        GameObject selected = eventSystem.currentSelectedGameObject;
        if (selected == null) return null;
        return selected.GetComponentInParent<Slider>() ?? selected.GetComponent<Slider>();
    }


    private void Awake()
    {
        enabled = false;
    }
}

