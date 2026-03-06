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
	[Tooltip("Ordered list of selectables for gamepad navigation (top to bottom).")]
	[SerializeField] private Selectable[] selectables;
	public Selectable[] Selectables => selectables;

	[Tooltip("Index in the selectables array to select by default when the menu opens.")]
	[SerializeField] private int defaultSelectedIndex = 1;
	public int DefaultSelectedIndex => defaultSelectedIndex;

	[Tooltip("The close (X) button in the top right")]
	[SerializeField] private Button closeButton;

	[Tooltip("The credits panel that may be open when the menu closes.")]
	[SerializeField] private GameObject creditsPanel;

	/// <summary>
	/// Fired when the settings menu is closed (by the close button or cancel).
	/// </summary>
	public event Action Closed;

	public void ShowSettings(bool on = true) => settings.SetActive(on);

	private void Start()
	{
		if (closeButton != null)
			closeButton.onClick.AddListener(Close);
	}

	private void Update()
	{
		// Handle mouse click on the close button manually since there is no InputModule for pointer events.
		if (!settings.activeSelf) return;
		if (closeButton == null) return;
		if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

		Vector2 mousePos = Mouse.current.position.ReadValue();
		RectTransform closeRect = closeButton.GetComponent<RectTransform>();

		if (RectTransformUtility.RectangleContainsScreenPoint(closeRect, mousePos))
			Close();
	}

	/// <summary>
	/// Opens the settings menu and binds input to the given player.
	/// </summary>
	public void Open(PlayerInput playerInput)
	{
		settings.SetActive(true);
		inputHandler.Bind(playerInput, eventSystem, this);
	}

	/// <summary>
	/// Closes the settings menu and unbinds input.
	/// </summary>
	public void Close()
	{
		// Make sure credits panel is closed
		if (creditsPanel != null)
			creditsPanel.SetActive(false);

		inputHandler.Unbind();
		eventSystem.SetSelectedGameObject(null);
		settings.SetActive(false);
		Closed?.Invoke();
	}
}
