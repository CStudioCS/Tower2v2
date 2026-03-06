using UnityEngine;

public class SettingsMenu: MonoBehaviour
{
	[SerializeField] private GameObject settings;

	public void ShowSettings(bool on = true) => settings.SetActive(on);
}
