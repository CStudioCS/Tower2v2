using UnityEngine;

public class Recipe : MonoBehaviour
{
    [SerializeField] private Item.Type type;
    public Item.Type Type => type;
    
    [SerializeField] private float transitionTime = .1f;
    
    [SerializeField] private RectTransform rectTransform;
    
    private RecipeSlot slotTarget;
    private Vector2 targetPosition;
    private float targetScale;
    
    private Vector2 velocity;
    private float scaleVelocity;

    public void Appear(RecipeSlot slot, bool animate = false)
    {
        SetSlotAsTarget(slot);
        ReachTargetPosition();
        
        if (animate)
            SetScale(0);
        else
            ReachTargetScale();
    }

    public void MoveToRecipeSlot(RecipeSlot slot)
    {
        SetSlotAsTarget(slot);
    }

    private void SetSlotAsTarget(RecipeSlot slot)
    {
        slotTarget = slot;
        targetPosition = slot.RecipePosition;
        targetScale = slot.RecipeScale;
    }

    private void SetPosition(Vector2 position) => rectTransform.position = position;
    private void SetScale(float scale) => rectTransform.localScale = scale * Vector3.one;

    private void ReachTargetPosition() => SetPosition(targetPosition);
    private void ReachTargetScale() => SetScale(targetScale);

    public void Disappear(bool animate = false)
    {
        if (animate)
        {
            targetScale = 0;
            Destroy(gameObject, transitionTime * 2);
        }
        else Destroy(gameObject);
    }

    private void Update()
    {
        // The use of SmoothDamp is not recommended (changing manually the value each frame, low control on the easing.)
        // But for now, it's the simplest solution and gives pretty good results.
        // Ideally, we should do this using an external package, but I couldn't find a package that does exactly what I wanted.
        // So ideally, we should make our own external package, that probably does something like this video: https://youtu.be/KPoeNZZ6H4s?si=l3mShw5QepdsIROI
        // But this would take a lot of time, so let's use this one-liner to meet the deadline.
        rectTransform.position = Vector2.SmoothDamp(rectTransform.position, targetPosition, ref velocity, transitionTime);
        rectTransform.localScale = Vector3.one * Mathf.SmoothDamp(rectTransform.localScale.x, targetScale, ref scaleVelocity, transitionTime);
    }
}
