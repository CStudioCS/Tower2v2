using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressBarFill;

    private float maxProgressBarFill;

    private void Awake()
    {
        maxProgressBarFill = progressBarFill.sizeDelta.x;
        ResetProgress();
    }

    // The argument is expected in range 0 - 1
    public void UpdateProgress(float percentage) => SetProgress(Mathf.Lerp(0, maxProgressBarFill, percentage));

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
    
    // The argument progress here is expected in range 0 - MaxProgressBarFill
    private void SetProgress(float progress) => progressBarFill.sizeDelta = new Vector2(progress, progressBarFill.sizeDelta.y);
    public void SetProgressMax() => SetProgress(maxProgressBarFill);
    private void SetProgress0() => SetProgress(0);
}