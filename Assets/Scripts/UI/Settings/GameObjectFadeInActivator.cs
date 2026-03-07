using LitMotion;
using System.Collections;
using UnityEngine;

public class GameObjectFadeInActivator : GameObjectActivator
{
    private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration;
    private void Awake()
    {
        if (gameObjectToActivate.TryGetComponent<CanvasGroup>(out CanvasGroup cg))
            canvasGroup = cg;
        else
            Debug.LogError($"Object {gameObjectToActivate} you're trying to" +
                $"fade in using GameObjectFadeInActivator doesn't have a canvasGroup component !");
    }

    public override void ToggleActivate()
    {
        FadeInNOutUtility.FadeInOrOut(canvasGroup, fadeInDuration, !gameObjectToActivate.activeSelf, true);
    }
}
