using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
    [SerializeField] AudioSource inGameMusic;
    [SerializeField] AudioSource lobbyMusic;

    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEnded += OnGameEnded;
        lobbyMusic.Play();
    }

    private void OnGameAboutToStart()
    {
        lobbyMusic.Play();
        inGameMusic.Play();        
    }

    private void OnGameEnded()
    {
        lobbyMusic.Play();
        inGameMusic.Stop();
    }

    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }
}
