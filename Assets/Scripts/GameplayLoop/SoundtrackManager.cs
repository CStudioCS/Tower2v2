using System.Collections;
using LitMotion;
using UnityEngine;

public class SoundtrackManager : MonoBehaviour
{
    [SerializeField] AudioSource inGameMusic;
    [SerializeField] AudioSource lobbyMusic;
    [SerializeField] AudioSource endScreenMusic;

    [SerializeField] private float noneToLobbyFadeInDuration;
    [SerializeField] private Ease noneToLobbyFadeInEase = Ease.Linear;

    [SerializeField] private float lobbyToGameFadeInDuration;
    [SerializeField] private Ease lobbyToGameFadeInEase = Ease.Linear;
    [SerializeField] private float lobbyToGameFadeOutDuration;
    [SerializeField] private Ease lobbyToGameFadeOutEase = Ease.Linear;

    [SerializeField] private float gameToEndFadeInDuration;
    [SerializeField] private Ease gameToEndFadeInEase = Ease.Linear;
    [SerializeField] private float gameToEndFadeOutDuration;
    [SerializeField] private Ease gameToEndFadeOutEase = Ease.Linear;

    [SerializeField] private float endToLobbyFadeInDuration;
    [SerializeField] private Ease endToLobbyFadeInEase = Ease.Linear;
    [SerializeField] private float endToLobbyFadeOutDuration;
    [SerializeField] private Ease endToLobbyFadeOutEase = Ease.Linear;

    [SerializeField] private float pauseTransitionDuration;
    [SerializeField] private Ease pauseTransitionEase = Ease.Linear;
    [SerializeField] private float resumeTransitionDuration;
    [SerializeField] private Ease resumeTransitionEase = Ease.Linear;

    private float pausedGameTime;
    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEnded += OnGameEnded;
        LevelManager.Instance.ReturnedToLobby += OnBackToLobby;
        PauseMenu.instance.Paused += OnPaused;
        PauseMenu.instance.Resumed += OnResumed;
        SmoothMusicFadeIn(lobbyMusic, noneToLobbyFadeInDuration, noneToLobbyFadeInEase);
    }

    private void OnGameAboutToStart()
    {
        SmoothMusicFadeIn(inGameMusic, lobbyToGameFadeInDuration, lobbyToGameFadeInEase);
        StartCoroutine(SmoothMusicFadeOut(lobbyMusic, lobbyToGameFadeOutDuration, lobbyToGameFadeOutEase));
    }

    private void OnGameEnded()
    {
        StartCoroutine(StartEndMusicWithDelay());
    }

    private IEnumerator StartEndMusicWithDelay()
    {
        yield return new WaitForSeconds(3f);
        SmoothMusicFadeIn(endScreenMusic, gameToEndFadeInDuration, gameToEndFadeInEase);
        StartCoroutine(SmoothMusicFadeOut(inGameMusic, gameToEndFadeOutDuration, gameToEndFadeOutEase));
    }

    private void OnBackToLobby()
    {
        SmoothMusicFadeIn(lobbyMusic, endToLobbyFadeInDuration, endToLobbyFadeInEase);
        StartCoroutine(SmoothMusicFadeOut(endScreenMusic, endToLobbyFadeOutDuration, endToLobbyFadeOutEase));
    }
    
    private void OnPaused()
    {
        pausedGameTime = inGameMusic.time;
        SmoothMusicFadeIn(lobbyMusic, pauseTransitionDuration, pauseTransitionEase);
        StartCoroutine(SmoothMusicFadeOut(inGameMusic, pauseTransitionDuration, pauseTransitionEase));
    }

    private void OnResumed()
    {
        float rollbackTime = resumeTransitionDuration * 0.5f;
        float targetTime = pausedGameTime - rollbackTime;
        if (targetTime < 0) targetTime = 0;
        inGameMusic.time = targetTime;
        SmoothMusicFadeIn(inGameMusic, resumeTransitionDuration, resumeTransitionEase);
        StartCoroutine(SmoothMusicFadeOut(lobbyMusic, resumeTransitionDuration, resumeTransitionEase));
    }

    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.Instance.GameEnded -= OnGameEnded;
        LevelManager.Instance.ReturnedToLobby -= OnBackToLobby;
        PauseMenu.instance.Paused -= OnPaused;
        PauseMenu.instance.Resumed -= OnResumed;
    }

    private void SmoothMusicFadeIn(AudioSource targetAudioSource,float transitionTime, Ease ease)
    {
        targetAudioSource.volume = 0;
        targetAudioSource.Play();
        LMotion.Create(0f, 1f, transitionTime).WithEase(ease).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale).Bind(volume => targetAudioSource.volume = volume);
    }
    private IEnumerator SmoothMusicFadeOut(AudioSource actualAudioSource, float transitionTime, Ease ease)
    {
        LMotion.Create(1f, 0f, transitionTime).WithEase(ease).WithScheduler(MotionScheduler.UpdateIgnoreTimeScale).Bind(volume => actualAudioSource.volume = volume);
        yield return new WaitForSecondsRealtime(transitionTime);
        actualAudioSource.Stop();
    }
}
