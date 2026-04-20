using System.Collections;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject view;

    private void OnEnable()
    {
        LevelManager.GameStarted += OnGameStarted;
        LevelManager.GameEndedOrReturnedToLobby += OnGameEnded;
    }

    private void OnDisable()
    {
        LevelManager.GameStarted -= OnGameStarted;
        LevelManager.GameEndedOrReturnedToLobby -= OnGameEnded;
    }

    private void OnGameStarted() => animator.SetBool("active", true);

    private void OnGameEnded() => animator.SetBool("active", false);
}
