using UnityEngine;

public class InLobbyActivator: MonoBehaviour
{
	[SerializeField] private GameObject gameObjectToActivate;
	[Tooltip("If activeInLobby is true: game object is active in lobby and inactive in gameplay.\nIf activeInLobby is false: game object is active in gameplay and inactive in lobby.")]
	[SerializeField] private bool activeInLobby = true;
	
	public void Start()
	{
		LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
		LevelManager.Instance.ReturnedToLobby += OnReturnedToLobby;
		Activate(activeInLobby);
	}

	private void OnGameAboutToStart() => Activate(!activeInLobby);

	private void OnReturnedToLobby() => Activate(activeInLobby);

	private void Activate(bool on = true) => gameObjectToActivate.SetActive(on);

	private void OnDisable()
	{
		LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
		LevelManager.Instance.ReturnedToLobby -= OnReturnedToLobby;
	}
}
