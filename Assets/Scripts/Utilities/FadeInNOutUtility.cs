using LitMotion;
using UnityEngine;

public static class FadeInNOutUtility
{
	public static MotionHandle FadeInOrOut(CanvasGroup canvasGroup, float time, bool fadeIn, bool setActive = true, bool fromCurrentValue = false)
	{
		if (fadeIn && setActive)
			canvasGroup.gameObject.SetActive(true);

		return LMotion.Create(fromCurrentValue ? canvasGroup.alpha : fadeIn ? 0f : 1f, fadeIn ? 1f : 0f, time)
					  .WithOnComplete(() =>
					  {
						  if (!fadeIn && setActive)
							  canvasGroup.gameObject.SetActive(false);
					  })
					  .Bind(a => canvasGroup.alpha = a);
	}
}
