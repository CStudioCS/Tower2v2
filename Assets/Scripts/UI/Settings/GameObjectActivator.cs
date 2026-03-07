using UnityEngine;

public class GameObjectActivator: MonoBehaviour
{
	[SerializeField] protected GameObject gameObjectToActivate;
	
	public virtual void ToggleActivate() => gameObjectToActivate.SetActive(!gameObjectToActivate.activeSelf);
}
