using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Manually drives UI navigation for the settings menu using a specific player's input actions.
/// This replaces InputSystemUIInputModule for per-player UI control in local multiplayer.
/// Navigation is index-based using an ordered list of selectables from SettingsMenu.
/// When a Slider is selected, left/right adjusts its value instead of navigating away.
/// Supports seamless switching between gamepad and mouse: only one controls selection at a time.
/// </summary>
public class SettingsMenuInputHandler: MonoBehaviour
{
	[Tooltip("Delay before repeated navigation when holding the stick")]
	[SerializeField] private float repeatDelay = 0.4f;
	[Tooltip("Rate of repeated navigation while holding the stick")]
	[SerializeField] private float repeatRate = 0.12f;
	[Tooltip("Stick deadzone for navigation")]
	[SerializeField] private float deadzone = 0.5f;
	[Tooltip("How fast the slider moves per second when holding the stick")]
	[SerializeField] private float sliderSpeed = 1f;
	[Tooltip("Minimum mouse delta (pixels) to count as intentional mouse movement")]
	[SerializeField] private float mouseMovementThreshold = 2f;

	private InputAction navigateAction;
	private InputAction submitAction;
	private InputAction cancelAction;

	private EventSystem eventSystem;
	private SettingsMenu settingsMenu;
	private Selectable[] selectables;
	private int currentIndex;
	private float nextMoveTime;
	private Vector2 lastDirection;

	// Reusable event data to avoid GC allocs.
	private PointerEventData cachedPointerData;

	// True  = our handler drives selection (gamepad/keyboard); module is OFF.
	// False = the mouse / InputModule drives selection; module is ON.
	private bool gamepadHasControl;
	private Vector2 lastMousePosition;
	private bool wasNavigatingLastFrame;

	// The scene's InputSystemUIInputModule — toggled on/off with mode switches.
	private InputSystemUIInputModule uiModule;
	// Original enabled state of the module's move/submit/cancel actions so we
	// can restore them on Unbind.
	private bool hadMoveAction;
	private bool hadSubmitAction;
	private bool hadCancelAction;

	// If the SettingsPanel has its own EventSystem we disable it so only
	// the scene's EventSystem is active (Unity only supports one).
	private EventSystem disabledExtraEventSystem;

	// ───────────────────────────────────────────────────────────────────
	//  Bind / Unbind
	// ───────────────────────────────────────────────────────────────────

	/// <summary>
	/// Binds this handler to a specific player's input actions and event system.
	/// </summary>
	public void Bind(PlayerInput playerInput, EventSystem targetEventSystem, SettingsMenu menu)
	{
		settingsMenu = menu;
		selectables = menu.Selectables;

		navigateAction = playerInput.actions.FindAction("UI/Navigate");
		submitAction = playerInput.actions.FindAction("UI/Submit");
		cancelAction = playerInput.actions.FindAction("UI/Cancel");

		nextMoveTime = 0f;
		lastDirection = Vector2.zero;
		wasNavigatingLastFrame = false;
		lastMousePosition = Mouse.current != null
			? Mouse.current.position.ReadValue()
			: Vector2.zero;

		// --- Make sure only ONE EventSystem is active.  The SettingsPanel
		//     prefab carries its own bare EventSystem; if that's different
		//     from the scene's main one we disable it to avoid conflicts.
		//     We capture EventSystem.current BEFORE disabling the module so
		//     that the scene's EventSystem is still "current".
		eventSystem = EventSystem.current != null ? EventSystem.current : targetEventSystem;
		disabledExtraEventSystem = null;
		if (targetEventSystem != null && targetEventSystem != eventSystem)
		{
			targetEventSystem.gameObject.SetActive(false);
			disabledExtraEventSystem = targetEventSystem;
		}

		cachedPointerData = new PointerEventData(eventSystem);

		// --- Find the scene's InputSystemUIInputModule (it may be on a
		//     different EventSystem than the one SettingsMenu references).
		uiModule = FindAnyObjectByType<InputSystemUIInputModule>();
		if (uiModule != null)
		{
			hadMoveAction = DisableModuleAction(uiModule.move);
			hadSubmitAction = DisableModuleAction(uiModule.submit);
			hadCancelAction = DisableModuleAction(uiModule.cancel);

			// Start in gamepad mode → module fully OFF so no pointer events
			// can create a second highlight.
			uiModule.enabled = false;
		}

		// Clear any lingering pointer hover from before the menu opened.
		eventSystem.SetSelectedGameObject(null);
		ClearAllPointerState();

		// Select the default selectable.
		gamepadHasControl = true;
		currentIndex = Mathf.Clamp(menu.DefaultSelectedIndex, 0, selectables.Length - 1);

		if (selectables.Length > 0)
			SelectCurrent();

		enabled = true;
	}

	/// <summary>
	/// Unbinds this handler.
	/// </summary>
	public void Unbind()
	{
		// Restore the InputModule.
		if (uiModule != null)
		{
			RestoreModuleAction(uiModule.move, hadMoveAction);
			RestoreModuleAction(uiModule.submit, hadSubmitAction);
			RestoreModuleAction(uiModule.cancel, hadCancelAction);
			uiModule.enabled = true; // always re-enable on close
			uiModule = null;
		}

		// Re-enable the extra EventSystem if we disabled it.
		if (disabledExtraEventSystem != null)
		{
			disabledExtraEventSystem.gameObject.SetActive(true);
			disabledExtraEventSystem = null;
		}

		navigateAction = null;
		submitAction = null;
		cancelAction = null;
		eventSystem = null;
		settingsMenu = null;
		selectables = null;
		cachedPointerData = null;
		enabled = false;
	}

	// ───────────────────────────────────────────────────────────────────
	//  Update
	// ───────────────────────────────────────────────────────────────────

	private void Update()
	{
		if (eventSystem == null)
			return;

		DetectInputSwitch();

		if (gamepadHasControl)
		{
			// Enforce our tracked selection every frame.
			if (selectables != null && selectables.Length > 0)
			{
				GameObject expected = selectables[currentIndex].gameObject;
				if (eventSystem.currentSelectedGameObject != expected)
					eventSystem.SetSelectedGameObject(expected);
			}

			// Continuously scrub any pointer-hover highlight every frame.
			// Even with the module disabled, residual isPointerInside flags
			// from before the switch can survive, so we clear them always.
			ClearAllPointerState();
		}

		HandleNavigation();
		HandleSubmit();
		HandleCancel();
	}

	// ───────────────────────────────────────────────────────────────────
	//  Mouse / Gamepad switching
	// ───────────────────────────────────────────────────────────────────

	private void DetectInputSwitch()
	{
		// --- Mouse activity ------------------------------------------------
		bool mouseActed = false;
		if (Mouse.current != null)
		{
			Vector2 mousePos = Mouse.current.position.ReadValue();
			Vector2 delta = mousePos - lastMousePosition;
			lastMousePosition = mousePos;

			if (delta.sqrMagnitude > mouseMovementThreshold * mouseMovementThreshold
			 || Mouse.current.leftButton.wasPressedThisFrame)
			{
				mouseActed = true;
			}
		}

		// --- Gamepad / bound-player activity (rising-edge only) ------------
		bool isNavigating = false;
		if (navigateAction != null)
		{
			Vector2 nav = navigateAction.ReadValue<Vector2>();
			isNavigating = nav.magnitude >= deadzone;
		}

		bool gamepadActed = isNavigating && !wasNavigatingLastFrame;
		wasNavigatingLastFrame = isNavigating;

		if (!gamepadActed && submitAction != null && submitAction.WasPressedThisFrame())
			gamepadActed = true;
		if (!gamepadActed && cancelAction != null && cancelAction.WasPressedThisFrame())
			gamepadActed = true;

		// --- Switch to MOUSE mode ------------------------------------------
		if (mouseActed && !gamepadActed && gamepadHasControl)
		{
			gamepadHasControl = false;

			// Enable the InputModule so pointer events work.
			if (uiModule != null)
				uiModule.enabled = true;

			// Deselect the gamepad-tracked button so it doesn't stay
			// in "Selected" visual while the mouse highlights another.
			eventSystem.SetSelectedGameObject(null);
		}

		// --- Switch to GAMEPAD mode ----------------------------------------
		if (gamepadActed && !mouseActed && !gamepadHasControl)
		{
			gamepadHasControl = true;

			// Disable the InputModule so pointer events can't fire and
			// create a second highlighted button.
			if (uiModule != null)
				uiModule.enabled = false;

			// Sync our index to whatever the EventSystem currently has
			// selected (the button the mouse was on) so the transition
			// feels natural.
			SyncIndexToCurrentSelection();

			// Clear pointer hover that was set while the module was active.
			ClearAllPointerState();

			// Force-select so the visual updates immediately.
			SelectCurrent();
		}
	}

	// ───────────────────────────────────────────────────────────────────
	//  Helpers
	// ───────────────────────────────────────────────────────────────────

	private void SyncIndexToCurrentSelection()
	{
		GameObject current = eventSystem.currentSelectedGameObject;
		if (current == null)
			return;

		for (int i = 0; i < selectables.Length; i++)
		{
			if (selectables[i] != null && selectables[i].gameObject == current)
			{
				currentIndex = i;
				return;
			}
		}
	}

	/// <summary>
	/// Sends OnPointerExit to every selectable so none of them can show a
	/// "Highlighted" (hover) visual.
	/// </summary>
	private void ClearAllPointerState()
	{
		if (selectables == null || cachedPointerData == null)
			return;
		for (int i = 0; i < selectables.Length; i++)
		{
			Selectable sel = selectables[i];
			if (sel != null)
				ExecuteEvents.Execute(sel.gameObject, cachedPointerData,
									  ExecuteEvents.pointerExitHandler);
		}
	}

	private static bool DisableModuleAction(InputActionReference actionRef)
	{
		if (actionRef != null && actionRef.action != null && actionRef.action.enabled)
		{
			actionRef.action.Disable();
			return true;
		}

		return false;
	}

	private static void RestoreModuleAction(InputActionReference actionRef, bool wasEnabled)
	{
		if (wasEnabled && actionRef != null && actionRef.action != null)
			actionRef.action.Enable();
	}

	// ───────────────────────────────────────────────────────────────────
	//  Navigation / Submit / Cancel
	// ───────────────────────────────────────────────────────────────────

	private void HandleNavigation()
	{
		if (!gamepadHasControl)
			return;

		Vector2 input = navigateAction.ReadValue<Vector2>();

		if (input.magnitude < deadzone)
		{
			lastDirection = Vector2.zero;
			nextMoveTime = 0f;
			return;
		}

		// If on a slider and pushing left/right, adjust the slider value
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
			vertical = input.y > 0 ? -1 : 1;

		if (vertical == 0)
			return;

		bool isNewDirection = lastDirection == Vector2.zero
						   || Vector2.Dot(input.normalized, lastDirection.normalized) < 0.5f;
		nextMoveTime = Time.unscaledTime + (isNewDirection ? repeatDelay : repeatRate);
		lastDirection = input;

		int newIndex = currentIndex + vertical;
		if (newIndex < 0)
			newIndex = selectables.Length - 1;
		if (newIndex >= selectables.Length)
			newIndex = 0;

		currentIndex = newIndex;
		SelectCurrent();
	}

	private void HandleSubmit()
	{
		if (!gamepadHasControl)
			return;
		if (submitAction == null || !submitAction.WasPressedThisFrame())
			return;
		if (selectables == null || selectables.Length == 0)
			return;

		Selectable sel = selectables[currentIndex];
		if (sel != null)
			ExecuteEvents.Execute(sel.gameObject,
								  new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
	}

	private void HandleCancel()
	{
		if (cancelAction == null || !cancelAction.WasPressedThisFrame())
			return;
		settingsMenu.Close();
	}

	private void SelectCurrent()
	{
		if (selectables == null || selectables.Length == 0)
			return;
		eventSystem.SetSelectedGameObject(selectables[currentIndex].gameObject);
	}

	private Slider GetSelectedSlider()
	{
		if (selectables == null || selectables.Length == 0)
			return null;
		Selectable sel = selectables[currentIndex];
		if (sel == null)
			return null;
		return sel.GetComponentInParent<Slider>() ?? sel.GetComponent<Slider>();
	}


	private void Awake()
	{
		enabled = false;
	}
}
