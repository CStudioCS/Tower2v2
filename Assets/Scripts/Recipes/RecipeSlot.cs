using System;
using UnityEngine;
using UnityEngine.UI;

public class RecipeSlot : MonoBehaviour
{
    [SerializeField] private float referenceRecipeWidth = 100f;

    private bool initialized;
    private Vector2 recipePosition;
    public Vector2 RecipePosition
    {
        get
        {
            if (initialized) return recipePosition;
            Initialize();
            return recipePosition;
        }
    }
    
    private float recipeScale;
    public float RecipeScale
    {
        get
        {
            if (initialized) return recipeScale;
            Initialize();
            return recipeScale;
        }
    }
    
    [SerializeField] private RectTransform rectTransform;

    public event Action ResolutionChanged;

    private void Initialize()
    {
        // This looks scary, but this is forced to get the correct width of a child of a parent with a layout group.
        // It's fine because it's only called once in the initialization, and it avoids entering the width manually.
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
        
        recipeScale = 1920f / Screen.width * rectTransform.lossyScale.x * rectTransform.rect.width / referenceRecipeWidth;
        recipePosition = (Vector2) rectTransform.position + .5f * Screen.width / 1920f * new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        
        initialized = true;
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

    private System.Collections.IEnumerator ReinitializeAfterCanvasUpdate()
    {
        yield return new WaitForEndOfFrame(); 
        Initialize();
        ResolutionChanged?.Invoke();
    }
#endif
}