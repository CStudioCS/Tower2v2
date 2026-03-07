using LitMotion;
using System;
using UnityEngine;

public class OffTowerCounter : MonoBehaviour
{
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
        gameObject.SetActive(active);
        //TODO: put an animation
    }

    private void OnDisable()
    {
        LevelManager.Instance.SetActiveInGameUI -= DeactiveUIFromLevelManager;
    }
}
