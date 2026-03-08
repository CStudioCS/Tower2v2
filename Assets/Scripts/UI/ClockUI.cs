using System.Collections;
using UnityEngine;

public class ClockUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject view;

    private bool subscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void SetUIActive(bool active)
    {
        animator.SetBool("active", active);
    }

    private void OnDisable()
    {
        if (subscribed)
        {
            LevelManager.Instance.SetActiveInGameUI -= SetUIActive;
            subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if(LevelManager.Instance != null)
        {
            LevelManager.Instance.SetActiveInGameUI += SetUIActive;
            subscribed = true;
        }
    }
}
