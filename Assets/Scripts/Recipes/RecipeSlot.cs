using UnityEngine;
using UnityEngine.UI;

public class RecipeSlot : MonoBehaviour
{
    [SerializeField] private float referenceRecipeWidth = 100f;
    
    private float recipeScale;
    public float RecipeScale
    {
        get
        {
            if (recipeScale > 0) return recipeScale;
            
            // This looks scary, but this is forced to get the correct width of a child of a parent with a layout group.
            // It's fine because it's only called once in the initialization, and it avoids entering the width manually.
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform.parent.GetComponent<RectTransform>());
            recipeScale = rectTransform.rect.width / referenceRecipeWidth;
            return recipeScale;
        }
    }
    
    [SerializeField] private RectTransform rectTransform;
    public RectTransform RectTransform => rectTransform;

    private int recipeSlotIndex;
    
    public void SetRecipeSlotIndex(int index) => recipeSlotIndex = index;
}