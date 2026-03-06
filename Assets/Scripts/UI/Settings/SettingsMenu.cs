using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;

public class SettingsMenu: MonoBehaviour
{
	[SerializeField] private GameObject settings;
	[SerializeField] private EventSystem eventSystem;
	public EventSystem EventSystem => eventSystem;
	[SerializeField] private InputSystemUIInputModule inputSystemUIInputModule;
	public InputSystemUIInputModule InputSystemUIInputModule => inputSystemUIInputModule;

	public void ShowSettings(bool on = true) => settings.SetActive(on);
}
