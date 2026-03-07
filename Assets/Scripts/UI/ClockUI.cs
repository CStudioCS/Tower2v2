using UnityEngine;

public class ClockUI : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject view;

    private void Start()
    {
        LevelManager.Instance.SetActiveInGameUI += SetUIActive;
    }

    private void SetUIActive(bool active)
    {
        //view.SetActive(active);
        animator.SetBool("active", active);
    }

    private void OnDisable()
    {
        LevelManager.Instance.SetActiveInGameUI -= SetUIActive;
    }
}
