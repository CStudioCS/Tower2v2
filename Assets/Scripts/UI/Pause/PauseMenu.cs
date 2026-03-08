using System;
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
	public bool IsPaused { get; private set; }

	public event Action Paused;
	public event Action Resumed;
	
	public static PauseMenu instance;

	private bool PauseKeyPressed => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7);
	
	public void Awake()
	{
		if (instance != null)
			Destroy(instance);

		instance = this;
		pausePanel.SetActive(false);
	}
	
	private void Update()
	{
		if (!LevelManager.InGame)
			return;

		if (PauseKeyPressed)
		{
			if (IsPaused)
				Resume();
			else
				Pause();
			return;
		}
		
		if (IsPaused && eventSystem.currentSelectedGameObject == null && AnyInputPressed)
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
				if (gamepad.allControls.OfType<ButtonControl>().Any(button => button.wasPressedThisFrame))
					return true;
			}
			return false;
		}
	}

	public void Resume() => Resume(true);
	public void Resume(bool fireEvent)
	{
		pausePanel.SetActive(false);
		Time.timeScale = 1f;
		IsPaused = false;
		if (fireEvent)
			Resumed?.Invoke();
	}

	private void Pause()
	{
		pausePanel.SetActive(true);
		SelectFirstSelectable();
		Time.timeScale = 0f;
		IsPaused = true;
		Paused?.Invoke();
	}
	
	private void SelectFirstSelectable() => eventSystem.SetSelectedGameObject(firstSelectable.gameObject);
}
