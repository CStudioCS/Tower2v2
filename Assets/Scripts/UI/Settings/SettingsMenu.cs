using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SettingsMenu: MonoBehaviour
{
	[SerializeField] private GameObject settings;
	[SerializeField] private EventSystem eventSystem;
	public EventSystem EventSystem => eventSystem;
	[SerializeField] private SettingsMenuInputHandler inputHandler;
	public SettingsMenuInputHandler InputHandler => inputHandler;

	public void ShowSettings(bool on = true) => settings.SetActive(on);

	/// <summary>
	/// Opens the settings menu and binds input to the given player.
	/// </summary>
	public void Open(PlayerInput playerInput)
	{
		settings.SetActive(true);
		eventSystem.SetSelectedGameObject(eventSystem.firstSelectedGameObject);
		inputHandler.Bind(playerInput, eventSystem);
	}

	/// <summary>
	/// Closes the settings menu and unbinds input.
	/// </summary>
	public void Close()
	{
		inputHandler.Unbind();
		eventSystem.SetSelectedGameObject(null);
		settings.SetActive(false);
	}
}
