using UnityEngine;

public class PauseMenu: MonoBehaviour
{
	[SerializeField] private GameObject pausePanel;
	private bool isPaused;

	private bool PauseKeyPressed => Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7);
	
	private void Awake() => pausePanel.SetActive(false);
	
	private void Update()
	{
		if (!LevelManager.InGame)
			return;

		if (!PauseKeyPressed)
			return;

		if (isPaused)
			Resume();
		else
			Pause();
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
		Time.timeScale = 0f;
		isPaused = true;
	}
}
