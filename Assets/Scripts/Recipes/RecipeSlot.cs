using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSlot : MonoBehaviour
{
    [SerializeField] private float referenceRecipeWidth = 100f;

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform parentLayout;
    
    // Cached values — recomputed whenever the layout is dirty (on enable, on resolution change).
    private bool layoutDirty = true;
    private Vector2 cachedPosition;
    private float cachedScale;

    public Vector2 RecipePosition
    {
        get
        {
            if (layoutDirty) Recalculate();
            return cachedPosition;
        }
    }
    
    public float RecipeScale
    {
        get
        {
            if (layoutDirty) Recalculate();
            return cachedScale;
        }
    }

    public event Action ResolutionChanged;

    private void OnEnable()
    {
        layoutDirty = true;
        StartCoroutine(RecalculateAfterLayout());
    }

    private IEnumerator RecalculateAfterLayout()
    {
        // Wait one full frame so the Canvas layout system has completed its pass for newly activated UI.
        yield return null;
        
        // Force a fresh layout rebuild now that the Canvas has had time to settle.
        layoutDirty = true;
        Recalculate();
        ResolutionChanged?.Invoke();
    }
    
    private void Recalculate()
    {
        // Force the parent layout group to recalculate child sizes immediately.
        LayoutRebuilder.ForceRebuildLayoutImmediate(parentLayout);
        
        cachedScale = 1920f / Screen.width * rectTransform.lossyScale.x * rectTransform.rect.width / referenceRecipeWidth;
        cachedPosition = (Vector2) rectTransform.position + .5f * Screen.width / 1920f * new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        
        layoutDirty = false;
    }
    
#if UNITY_EDITOR
    int lastScreenWidth = Screen.width;
    
    private void Update()
    {
        // This is not the cleanest. In a real game, you would only allow resolution changes via the settings.
        // Here, I'm just preventing the bug when you change the resolution mid-game in the editor.
        if (Screen.width != lastScreenWidth) HandleResolutionChange();
    }

    private void HandleResolutionChange()
    {
        lastScreenWidth = Screen.width;
        StartCoroutine(ReinitializeAfterCanvasUpdate());
    }

    private IEnumerator ReinitializeAfterCanvasUpdate()
    {
        yield return new WaitForEndOfFrame();
        layoutDirty = true;
        Recalculate();
        ResolutionChanged?.Invoke();
    }
#endif
}