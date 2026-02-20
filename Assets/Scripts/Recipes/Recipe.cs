using UnityEngine;

public class Recipe : MonoBehaviour
{
    [SerializeField] private Item.Type type;
    public Item.Type Type => type;
    
    [SerializeField] private RectTransform rectTransform;
    
    private RecipeSlot slotTarget;

    public void SetRecipeSlot(RecipeSlot slot)
    {
        slotTarget = slot;
        MatchTarget();
    }
    
    private void MatchTarget()
    {
        rectTransform.position = slotTarget.RecipePosition;
        rectTransform.localScale = slotTarget.RecipeScale * Vector3.one;
    }
}
