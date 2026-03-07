using LitMotion;
using System;
using UnityEngine;

public static class FadeInNOutUtility
{
    /// <summary>
    /// this does a LMotion from 0 to 1 or 1 to 0 to set the alpha. Scale accordingly in setAlpha function
    /// </summary>
    public static void FadeInOrOut(Action<float> setAlpha, float time, bool fadeIN)
    {
        LMotion.Create(fadeIN ? 0f : 1f, fadeIN ? 1f : 0f, time).Bind(setAlpha);
    }
}
