using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenu: MonoBehaviour
{
	[SerializeField] private GameObject settings;
	[SerializeField] private EventSystem eventSystem;
	public EventSystem EventSystem => eventSystem;
	[SerializeField] private SettingsMenuInputHandler inputHandler;
	public SettingsMenuInputHandler InputHandler => inputHandler;

	[Header("Navigation")]
	[Tooltip("The close (X) button in the top right")]
	[SerializeField] private Button closeButton;
	[Tooltip("The first selectable in the vertical layout (e.g. the Slider)")]
	[SerializeField] private Selectable firstVerticalSelectable;
	[Tooltip("The last selectable in the vertical layout (e.g. the Quit button)")]
	[SerializeField] private Selectable lastVerticalSelectable;

	/// <summary>
	/// Fired when the settings menu is closed (by the close button or cancel).
	/// </summary>
	public event Action Closed;

	public void ShowSettings(bool on = true) => settings.SetActive(on);

	private void Start()
	{
		SetupNavigation();
		// Close button should call Close() when submitted via gamepad
		if (closeButton != null)
			closeButton.onClick.AddListener(Close);
	}

	/// <summary>
	/// Wire up explicit navigation so the close button is reachable from the vertical layout.
	/// CloseButton ↔ first item (up/down), last item → down wraps to CloseButton.
	/// </summary>
	private void SetupNavigation()
	{
		if (closeButton == null || firstVerticalSelectable == null) return;
		
		// CloseButton: down → first vertical item, up → last vertical item (wrap)
		Navigation closeNav = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnDown = firstVerticalSelectable,
			selectOnUp = lastVerticalSelectable != null ? lastVerticalSelectable : firstVerticalSelectable
		};
		closeButton.navigation = closeNav;

		// First vertical item: up → CloseButton (rest stays automatic via Explicit + FindSelectable)
		Navigation firstNav = new Navigation
		{
			mode = Navigation.Mode.Explicit,
			selectOnUp = closeButton,
			selectOnDown = firstVerticalSelectable.FindSelectableOnDown()
		};
		firstVerticalSelectable.navigation = firstNav;

		// Last vertical item: down → CloseButton (rest stays wired)
		if (lastVerticalSelectable != null && lastVerticalSelectable != firstVerticalSelectable)
		{
			Navigation lastNav = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = lastVerticalSelectable.FindSelectableOnUp(),
				selectOnDown = closeButton
			};
			lastVerticalSelectable.navigation = lastNav;
		}
	}

	/// <summary>
	/// Opens the settings menu and binds input to the given player.
	/// </summary>
	public void Open(PlayerInput playerInput)
	{
		settings.SetActive(true);
		eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
		inputHandler.Bind(playerInput, eventSystem, this);
	}

	/// <summary>
	/// Closes the settings menu and unbinds input.
	/// Called by the close button (via Inspector OnClick) or programmatically.
	/// </summary>
	public void Close()
	{
		inputHandler.Unbind();
		eventSystem.SetSelectedGameObject(null);
		settings.SetActive(false);
		Closed?.Invoke();
	}
}
