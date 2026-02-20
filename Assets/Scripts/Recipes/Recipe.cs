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
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(slotTarget.RectTransform.parent.GetComponent<RectTransform>());
        Vector2 pos = (Vector2)slotTarget.RectTransform.position + .5f * new Vector2(slotTarget.RectTransform.rect.width, slotTarget.RectTransform.rect.height);
        rectTransform.position = pos;
        rectTransform.localScale = slotTarget.RectTransform.lossyScale * slotTarget.RecipeScale;
    }
}
