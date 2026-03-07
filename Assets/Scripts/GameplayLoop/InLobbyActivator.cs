using UnityEngine;

public class InLobbyActivator: MonoBehaviour
{
	[SerializeField] private GameObject gameObjectToActivate;

	public void Start()
	{
		LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
		LevelManager.Instance.ReturnedToLobby += OnReturnedToLobby;
		Activate();
	}

	private void OnGameAboutToStart() => Activate(false);

	private void OnReturnedToLobby() => Activate();

	private void Activate(bool on = true) => gameObjectToActivate.SetActive(on);

	private void OnDisable()
	{
		LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
		LevelManager.Instance.ReturnedToLobby -= OnReturnedToLobby;
	}
}
