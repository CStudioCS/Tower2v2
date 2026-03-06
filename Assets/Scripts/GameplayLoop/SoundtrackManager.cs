using UnityEngine;
using LitMotion;
using LitMotion.Adapters;
using System.Collections;

public class SoundtrackManager : MonoBehaviour
{
    [SerializeField] AudioSource inGameMusic;
    [SerializeField] AudioSource lobbyMusic;
    [SerializeField] private float initFadeInDuration;
    [SerializeField] private Ease initFadeInEase = Ease.Linear;
    [SerializeField] private float startFadeInDuration;
    [SerializeField] private Ease startFadeInEase = Ease.Linear;
    [SerializeField] private float startFadeOutDuration;
    [SerializeField] private Ease startFadeOutEase = Ease.Linear;
    [SerializeField] private float endFadeInDuration;
    [SerializeField] private Ease endFadeInEase = Ease.Linear;
    [SerializeField] private float endFadeOutDuration;
    [SerializeField] private Ease endFadeOutEase = Ease.Linear;
    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEnded += OnGameEnded;
        SmoothMusicFadeIn(lobbyMusic, initFadeInDuration, initFadeInEase);
    }

    private void OnGameAboutToStart()
    {
        SmoothMusicFadeIn(inGameMusic, startFadeInDuration, startFadeInEase);
        StartCoroutine(SmoothMusicFadeOut(lobbyMusic, startFadeOutDuration, startFadeOutEase));
    }

    private void OnGameEnded()
    {
        SmoothMusicFadeIn(lobbyMusic, endFadeInDuration, endFadeInEase);
        StartCoroutine(SmoothMusicFadeOut(inGameMusic, endFadeOutDuration, endFadeOutEase));
    }

    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }

    private void SmoothMusicFadeIn(AudioSource targetAudioSource,float transitionTime, Ease ease)
    {
        targetAudioSource.volume = 0;
        targetAudioSource.Play();
        LMotion.Create(0f, 1f, transitionTime).WithEase(ease).Bind(volume => targetAudioSource.volume = volume);
    }
    private IEnumerator SmoothMusicFadeOut(AudioSource actualAudioSource, float transitionTime, Ease ease)
    {
        LMotion.Create(1f, 0f, transitionTime).WithEase(ease).Bind(volume => actualAudioSource.volume = volume);
        yield return new WaitForSeconds(transitionTime);
        actualAudioSource.Stop();
    }
}
