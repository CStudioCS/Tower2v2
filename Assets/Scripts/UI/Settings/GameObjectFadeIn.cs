using LitMotion;
using UnityEngine;

public class GameObjectFadeIn : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 0.2f;
    public float Duration => duration;

    public bool FadedIn { get; private set; }
    private MotionHandle handle;

    public void Fade()
    {
        FadedIn = !FadedIn;

        if (handle.IsActive()) handle.Cancel();

        handle = FadeInNOutUtility.FadeInOrOut(canvasGroup, duration, FadedIn, fromCurrentValue: true);
    }
}