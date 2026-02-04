using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressBarFill;
    [SerializeField] private Graphic[] graphics;
    
    private float maxProgressBarFill;

    // The argument is expected in range 0 - 1
    public void UpdateProgress(float percentage) => SetProgress(Mathf.Lerp(0, maxProgressBarFill, percentage));

    public void StartProgress()
    {
        SetProgress0();
        Show();
    }
    
    public void ResetProgress()
    {
        SetProgress0();
        Show(false);
    }
    
    // The argument progress here is expected in range 0 - MaxProgressBarFill
    private void SetProgress(float progress) => progressBarFill.sizeDelta = new Vector2(progress, progressBarFill.sizeDelta.y);
    public void SetProgressMax() => SetProgress(maxProgressBarFill);
    private void SetProgress0() => SetProgress(0);

    private void Show(bool on = true)
    {
        foreach (Graphic graphic in graphics)
            graphic.enabled = on;
    }

    private void Start()
    {
        maxProgressBarFill = progressBarFill.sizeDelta.x;
        ResetProgress();
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEnded += OnGameEnded;
    }
    
    private void OnGameAboutToStart() => ResetProgress();

    private void OnGameEnded() => ResetProgress();
    
    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }
}