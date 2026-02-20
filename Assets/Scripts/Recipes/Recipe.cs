using System.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class Recipe : MonoBehaviour
{
    [SerializeField] private Item.Type type;
    public Item.Type Type => type;
    
    [SerializeField] private float transitionTime = .1f;
    [SerializeField] private float popAnimationSemiDuration = .1f;
    [SerializeField] private float popAnimationScaleMultiplier = 2f;
    
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
        SetPosition(Vector2.SmoothDamp(rectTransform.position, targetPosition, ref velocity, transitionTime));
        SetScale(Mathf.SmoothDamp(rectTransform.localScale.x, targetScale, ref scaleVelocity, transitionTime));
    }
    
    private void ValidateRecipe()
    {
        _ = ValidateRecipeAsync();
    }

    private async Task ValidateRecipeAsync()
    {
        await LMotion.Create(Vector3.one, Vector3.one * popAnimationScaleMultiplier * targetScale, popAnimationSemiDuration)
            .WithEase(Ease.OutQuad)
            .BindToLocalScale(rectTransform);

        await LMotion.Create(Vector3.one * popAnimationScaleMultiplier, Vector3.zero, popAnimationSemiDuration)
            .WithEase(Ease.InQuad)
            .BindToLocalScale(rectTransform);
        
        Destroy(gameObject);
    }

    public void InvalidateRecipe()
    {
        _ = InvalidateRecipeAsync();
    }
    
    private async Task InvalidateRecipeAsync()
    {
        await LMotion.Create(Vector3.one, Vector3.one * popAnimationScaleMultiplier * targetScale, popAnimationSemiDuration)
            .WithEase(Ease.OutQuad) 
            .WithLoops(2, LoopType.Yoyo)
            .BindToLocalScale(rectTransform);
    }
}
