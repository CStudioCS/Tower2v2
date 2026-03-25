using UnityEngine;

public class MouseVisibilityManager: MonoBehaviour
{
	[SerializeField] private float movementThreshold = 0.01f;

	private void Awake() => Cursor.visible = false;

	void Update()
	{
		float mouseX = Input.GetAxis($"Mouse X");
		float mouseY = Input.GetAxis($"Mouse Y");

		if (!(Mathf.Abs(mouseX) > movementThreshold) && !(Mathf.Abs(mouseY) > movementThreshold))
			return;

		if (LevelManager.Instance.GameState != LevelManager.State.Lobby && !PauseMenu.instance.IsPaused)
			return;
		
		Cursor.visible = true;
	}
}
