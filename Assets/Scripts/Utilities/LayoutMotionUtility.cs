using UnityEngine;
using UnityEngine.UI;
using LitMotion;

public static class LayoutMotionUtility
{
    /// <summary>
    /// Animates the padding and spacing of a Vertical or Horizontal Layout Group.
    /// </summary>
    public static MotionHandle AnimateLayout(
        HorizontalOrVerticalLayoutGroup layoutGroup,
        int? targetLeft = null,
        int? targetRight = null,
        int? targetTop = null,
        int? targetBottom = null,
        float? targetSpacing = null,
        float duration = 0.3f,
        Ease easeType = Ease.OutQuart)
    {
        if (layoutGroup == null) return default;

        int startLeft = layoutGroup.padding.left;
        int startRight = layoutGroup.padding.right;
        int startTop = layoutGroup.padding.top;
        int startBottom = layoutGroup.padding.bottom;
        float startSpacing = layoutGroup.spacing;

        int endLeft = targetLeft ?? startLeft;
        int endRight = targetRight ?? startRight;
        int endTop = targetTop ?? startTop;
        int endBottom = targetBottom ?? startBottom;
        float endSpacing = targetSpacing ?? startSpacing;

        RectTransform rectTransform = layoutGroup.GetComponent<RectTransform>();

        return LMotion.Create(0f, 1f, duration)
            .WithEase(easeType)
            .Bind(t =>
            {
                layoutGroup.padding.left = (int)Mathf.Lerp(startLeft, endLeft, t);
                layoutGroup.padding.right = (int)Mathf.Lerp(startRight, endRight, t);
                layoutGroup.padding.top = (int)Mathf.Lerp(startTop, endTop, t);
                layoutGroup.padding.bottom = (int)Mathf.Lerp(startBottom, endBottom, t);

                layoutGroup.spacing = Mathf.Lerp(startSpacing, endSpacing, t);

                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            });
    }
}