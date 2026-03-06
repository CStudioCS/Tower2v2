using UnityEngine;
using LitMotion;
using LitMotion.Adapters;
using System.Collections;

public class SoundtrackManager : MonoBehaviour
{
    [SerializeField] AudioSource inGameMusic;
    [SerializeField] AudioSource lobbyMusic;
    [SerializeField] private float transitionTime;

    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEnded += OnGameEnded;
        StartCoroutine(SmoothMusicFadeIn(lobbyMusic));
    }

    private void OnGameAboutToStart()
    {
        StartCoroutine(SmoothMusicFadeIn(inGameMusic));  
        StartCoroutine(SmoothMusicFadeOut(lobbyMusic));
    }

    private void OnGameEnded()
    {
        StartCoroutine(SmoothMusicFadeIn(lobbyMusic));
        StartCoroutine(SmoothMusicFadeOut(inGameMusic));
    }

    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }

    private IEnumerator SmoothMusicFadeIn(AudioSource targetAudioSource)
    {
        targetAudioSource.Play();
        yield return LMotion.Create(0, 1, transitionTime).Bind(volume => targetAudioSource.volume = volume).ToYieldInstruction();
    }
    private IEnumerator SmoothMusicFadeOut(AudioSource actualAudioSource)
    {
        yield return LMotion.Create(1, 0, transitionTime).Bind(volume => actualAudioSource.volume = volume).ToYieldInstruction();
        actualAudioSource.Stop();
    }
}
