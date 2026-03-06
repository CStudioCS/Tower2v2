using UnityEngine;

public class GameObjectActivator: MonoBehaviour
{
	[SerializeField] private GameObject gameObjectToActivate;
	
	public void ToggleActivate() => gameObjectToActivate.SetActive(!gameObjectToActivate.activeSelf);
}
