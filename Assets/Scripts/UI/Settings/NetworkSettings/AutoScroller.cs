using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A utility component to mathematically force a ScrollRect to center/show a specific RectTransform.
/// Uses a smooth Lerp coroutine for premium visual feedback.
/// Independent from input logic.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class AutoScroller : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of the smooth scroll in seconds")]
    [SerializeField] private float scrollDuration = 0.1f;

    private ScrollRect scrollRect;
    private RectTransform contentRect;
    private RectTransform viewportRect;

    private Coroutine scrollCoroutine;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentRect = scrollRect.content;
        viewportRect = scrollRect.viewport != null ? scrollRect.viewport : GetComponent<RectTransform>();
    }

    /// <summary>
    /// Calculates the offset needed to make the target fully visible and triggers a smooth scroll.
    /// </summary>
    public void ScrollToTarget(RectTransform targetRect)
    {
        if (targetRect == null || contentRect == null || viewportRect == null) return;

        // Safety lock: only scroll if the target is actually inside the content panel
        if (!targetRect.IsChildOf(contentRect)) return;

        Bounds targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewportRect, targetRect);

        float viewportTop = viewportRect.rect.yMax;
        float viewportBottom = viewportRect.rect.yMin;
        float offset = 0f;

        if (targetBounds.min.y < viewportBottom)
        {
            offset = viewportBottom - targetBounds.min.y;
        }
        else if (targetBounds.max.y > viewportTop)
        {
            offset = viewportTop - targetBounds.max.y;
        }

        if (offset != 0f)
        {
            // Kill residual physics momentum to prevent fighting the Lerp
            scrollRect.velocity = Vector2.zero;

            // Calculate the final destination
            Vector2 targetPosition = contentRect.anchoredPosition;
            targetPosition.y += offset;

            // Stop any ongoing scroll animation before starting a new one
            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
            }

            // Start the smooth scroll
            if (gameObject.activeInHierarchy)
            {
                scrollCoroutine = StartCoroutine(SmoothScroll(targetPosition));
            }
            else
            {
                // Fallback if the object is disabled mid-frame
                contentRect.anchoredPosition = targetPosition;
            }
        }
    }

    /// <summary>
    /// Smoothly interpolates the content's anchored position over time.
    /// </summary>
    private IEnumerator SmoothScroll(Vector2 targetPosition)
    {
        Vector2 startPosition = contentRect.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < scrollDuration)
        {
            // Using unscaledDeltaTime so the menu works even if the game is paused (Time.timeScale = 0)
            elapsedTime += Time.unscaledDeltaTime;

            // Calculate percentage completed
            float t = elapsedTime / scrollDuration;

            // Apply easing for a smoother start/stop (organic feel)
            t = Mathf.SmoothStep(0f, 1f, t);

            // Apply position
            contentRect.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        // Snap to exact target position at the very end to avoid floating point inaccuracies
        contentRect.anchoredPosition = targetPosition;
        scrollCoroutine = null;
    }
}