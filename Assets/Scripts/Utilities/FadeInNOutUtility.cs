using LitMotion;
using System;
using UnityEngine;

public static class FadeInNOutUtility
{
    public static async void FadeInOrOut(CanvasGroup canvasGroup, float time, bool fadeIn, bool setActive = true)
    {
        if (fadeIn && setActive) canvasGroup.gameObject.SetActive(true);

        await LMotion.Create(fadeIn ? 0f : 1f, fadeIn ? 1f : 0f, time).Bind((alpha) => canvasGroup.alpha = alpha);

        if (!fadeIn && setActive) canvasGroup.gameObject.SetActive(false);
    }
}
