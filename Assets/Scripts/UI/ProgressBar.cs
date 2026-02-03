using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressBarFill;

    private float maxProgressBarFill;
    private float MaxProgressBarFill => maxProgressBarFill == 0 ? maxProgressBarFill = progressBarFill.sizeDelta.x : maxProgressBarFill;
    
    private Coroutine currentCoroutine;

    private void Awake() => ResetProgress();

    public void UpdateProgress(float percentage)
    {
        progressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, MaxProgressBarFill, percentage), progressBarFill.sizeDelta.y);
    }

    public void StartProgress()
    {
        SetProgress0();
        gameObject.SetActive(true);
    }
    
    public void ResetProgress()
    {
        SetProgress0();
        gameObject.SetActive(false);
    }
    
    private void SetProgress0() => progressBarFill.sizeDelta = new Vector2(0, progressBarFill.sizeDelta.y);
}