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

    private void Initialize()
    {
        // This looks scary, but this is forced to get the correct width of a child of a parent with a layout group.
        // It's fine because it's only called once in the initialization, and it avoids entering the width manually.
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
        
        recipeScale = rectTransform.lossyScale.x * rectTransform.rect.width / referenceRecipeWidth;
        recipePosition = (Vector2) rectTransform.position + .5f * new Vector2(rectTransform.rect.width, rectTransform.rect.height);
        
        initialized = true;
    }
}