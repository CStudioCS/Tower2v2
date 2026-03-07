using LitMotion;
using System;
using UnityEngine;

public static class FadeInNOutUtility
{
    public static async void FadeInOrOut(CanvasGroup canvasGroup, float time, bool fadeIN, bool setActive = true)
    {
        if (fadeIN && setActive) canvasGroup.gameObject.SetActive(true);

        await LMotion.Create(fadeIN ? 0f : 1f, fadeIN ? 1f : 0f, time).Bind((alpha) => canvasGroup.alpha = alpha);

        if (!fadeIN && setActive) canvasGroup.gameObject.SetActive(false);
    }
}
