using UnityEngine;

public class GameObjectActivator: MonoBehaviour
{
	public GameObject GameObjectToActivate;
	
	public virtual void ToggleActivate() => GameObjectToActivate.SetActive(!GameObjectToActivate.activeSelf);
}
