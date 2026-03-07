using LitMotion;
using System;
using UnityEngine;

public class OffTowerCounter : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private void Start()
    {
        LevelManager.Instance.SetActiveInGameUI += DeactiveUIFromLevelManager;
    }

    private void DeactiveUIFromLevelManager(bool active)
    {
        if(!active) SetUIActive(false);
    }

    public void SetUIActive(bool active)
    {
        //TODO: put an animation
        animator.SetBool("active", active);
    }

    private void OnDisable()
    {
        LevelManager.Instance.SetActiveInGameUI -= DeactiveUIFromLevelManager;
    }
}
