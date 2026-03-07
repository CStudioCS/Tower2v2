using LitMotion;
using System.Collections;
using UnityEngine;

public class GameObjectFadeIn : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration;

    public bool FadedIn { get; private set; }

    private void Update()
    {
        canvasGroup.alpha += (FadedIn ? 1f : -1f) * Time.deltaTime / fadeInDuration;
    }

    public void Fade()
    {
        FadedIn = !FadedIn;
    }
}
