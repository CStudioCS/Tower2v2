using UnityEngine;

public class ApplicationQuitter: MonoBehaviour
{
	public void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.ExitPlaymode();
#endif
		Application.Quit();
	}
}