using LitMotion;
using System;
using UnityEngine;

public class OffTowerCounter : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool subscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed || LevelManager.Instance == null)
            return;

        subscribed = true;
        LevelManager.Instance.SetActiveInGameUI += DeactiveUIFromLevelManager;
    }

    private void DeactiveUIFromLevelManager(bool active)
    {
        if(!active) SetUIActive(false);
    }

    public void SetUIActive(bool active)
    {
        animator.SetBool("active", active);
    }

    private void OnDisable()
    {
        if (subscribed)
        {
            LevelManager.Instance.SetActiveInGameUI -= DeactiveUIFromLevelManager;
            subscribed = false;
        }
    }
}
