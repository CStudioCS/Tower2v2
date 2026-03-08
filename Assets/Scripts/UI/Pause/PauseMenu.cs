using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class PauseMenu: MonoBehaviour
{
	[SerializeField] private GameObject pausePanel;
	[SerializeField] private EventSystem eventSystem;
	[SerializeField] private Selectable firstSelectable;
	private bool isPaused;

	private bool PauseKeyPressed => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7);
	
	private void Awake() => pausePanel.SetActive(false);
	
	private void Update()
	{
		if (!LevelManager.InGame)
			return;

		if (PauseKeyPressed)
		{
			if (isPaused)
				Resume();
			else
				Pause();
			return;
		}
		
		if (isPaused && eventSystem.currentSelectedGameObject == null && AnyInputPressed)
		{
			SelectFirstSelectable();
		}
	}

	private bool AnyInputPressed
	{
		get
		{
			if (Input.anyKeyDown)
				return true;

			foreach (Gamepad gamepad in Gamepad.all)
			{
				if (gamepad.allControls.OfType<ButtonControl>().Any(button => button.isPressed))
					return true;
			}
			return false;
		}
	}

	public void Resume()
	{
		pausePanel.SetActive(false);
		Time.timeScale = 1f;
		isPaused = false;
	}

	private void Pause()
	{
		pausePanel.SetActive(true);
		SelectFirstSelectable();
		Time.timeScale = 0f;
		isPaused = true;
	}
	
	private void SelectFirstSelectable() => eventSystem.SetSelectedGameObject(firstSelectable.gameObject);
}
