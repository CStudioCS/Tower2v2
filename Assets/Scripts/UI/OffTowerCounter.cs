using TMPro;
using UnityEngine;

public class OffTowerCounter : MonoBehaviour
{
    private static readonly int Active = Animator.StringToHash("active");
    [SerializeField] private Animator animator;
    private bool subscribed;
    
    [SerializeField] private TextMeshProUGUI text;

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
        LevelManager.FewSecondsBeforeGameEnded += DeactiveUIFromLevelManager;
    }

    private void DeactiveUIFromLevelManager() => SetUIActive(false);

    public void SetUIActive(bool active) => animator.SetBool(Active, active);

    public void SetText(string message) => text.text = message;

    private void OnDisable()
    {
        if (subscribed)
        {
            LevelManager.FewSecondsBeforeGameEnded -= DeactiveUIFromLevelManager;
            subscribed = false;
        }
    }
}
