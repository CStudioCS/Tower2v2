using UnityEngine;

public class GameObjectActivator: MonoBehaviour
{
	[SerializeField] private GameObject GameObjectToActivate;
	
	public void ToggleActivate() => GameObjectToActivate.SetActive(!GameObjectToActivate.activeSelf);
}
