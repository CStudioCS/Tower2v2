using System;
using UnityEngine;
using UnityEngine.EventSystems;
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
	private bool toggledPauseThisFrame;

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

		if (IsPaused && eventSystem.currentSelectedGameObject == null && InputUtility.AnyInputPressed)
			SelectFirstSelectable();
	}

	private void LateUpdate() => toggledPauseThisFrame = false;

	public void TogglePause()
	{
		if(toggledPauseThisFrame) return;
		toggledPauseThisFrame = true;

		if (IsPaused)
			Resume();
		else
			Pause();
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
