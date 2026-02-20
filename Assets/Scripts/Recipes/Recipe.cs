using System;
using System.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class Recipe : MonoBehaviour
{
    [SerializeField] private Item.Type type;
    public Item.Type Type => type;
    
    [SerializeField] private float transitionTime = .1f;
    [SerializeField] private float popAnimationSemiDuration = .2f;
    [SerializeField] private float popValidateAnimationScaleMultiplier = 1.5f;
    [SerializeField] private float popInvalidateAnimationScaleMultiplier = 1.5f;
    
    [SerializeField] private float delayBeforeMove = 0.3f;
    
    [SerializeField] private RectTransform rectTransform;
    
    private RecipeSlot slotTarget;
    private Vector2 targetPosition;
    private float targetScale;
    
    private Vector2 velocity;
    private float scaleVelocity;

    private MotionHandle scaleTweenHandle;
    private bool isAnimatingScale;
    private bool isDestroying;
    
    private int currentMoveCommandId = 0;

    public void Appear(RecipeSlot slot, bool animate = false)
    {
        if (animate)
        {
            SetScale(0);
            slotTarget = slot;
            targetPosition = slotTarget.RecipePosition;
            targetScale = 0; 
            ReachTargetPosition();
            MoveToRecipeSlot(slot);
        }
        else
        {
            SetSlotAsTarget(slot);
            ReachTargetPosition();
            ReachTargetScale();
        }
    }

    public void MoveToRecipeSlot(RecipeSlot slot)
    {
        currentMoveCommandId++;
        _ = MoveToRecipeSlotAsync(slot, currentMoveCommandId);
    }

    private async Task MoveToRecipeSlotAsync(RecipeSlot slot, int commandId)
    {
        if (delayBeforeMove > 0f)
        {
            await Task.Delay(Mathf.RoundToInt(delayBeforeMove * 1000f));
            if (this == null || gameObject == null) return;
            if (currentMoveCommandId != commandId) return;
        }

        SetSlotAsTarget(slot);
    }

    private void SetSlotAsTarget(RecipeSlot slot)
    {
        slotTarget = slot;
        targetPosition = slotTarget.RecipePosition;
        targetScale = slotTarget.RecipeScale;
    }

    private void SetPosition(Vector2 position) => rectTransform.position = position;
    private void SetScale(float scale) => rectTransform.localScale = scale * Vector3.one;

    private void ReachTargetPosition() => SetPosition(targetPosition);
    private void ReachTargetScale() => SetScale(targetScale);

    public void Disappear(bool animate = false)
    {
        if (isDestroying) return;
        isDestroying = true;
        
        if (animate)
        {
            targetScale = 0;
            CancelCurrentAnimation(); 
            Destroy(gameObject, transitionTime * 2);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        SetPosition(Vector2.SmoothDamp(rectTransform.position, targetPosition, ref velocity, transitionTime));
        
        if (!isAnimatingScale)
        {
            SetScale(Mathf.SmoothDamp(rectTransform.localScale.x, targetScale, ref scaleVelocity, transitionTime));
        }
    }

    private void CancelCurrentAnimation()
    {
        if (scaleTweenHandle.IsActive())
        {
            scaleTweenHandle.Cancel();
        }
        isAnimatingScale = false;
    }
    
    public void ValidateRecipe()
    {
        if (isDestroying) return; 
        isDestroying = true;
        
        _ = ValidateRecipeAsync();
    }

    private async Task ValidateRecipeAsync()
    {
        CancelCurrentAnimation();
        isAnimatingScale = true;

        Vector3 startScale = rectTransform.localScale;
        Vector3 peakScale = Vector3.one * (popValidateAnimationScaleMultiplier * targetScale);

        try
        {
            scaleTweenHandle = LMotion.Create(startScale, peakScale, popAnimationSemiDuration)
                .WithEase(Ease.OutQuad)
                .BindToLocalScale(rectTransform);

            await scaleTweenHandle;

            scaleTweenHandle = LMotion.Create(peakScale, Vector3.zero, popAnimationSemiDuration)
                .WithEase(Ease.InQuad)
                .BindToLocalScale(rectTransform);

            await scaleTweenHandle;
            
            if (gameObject != null) Destroy(gameObject);
        }
        catch (OperationCanceledException) { }
    }

    public void InvalidateRecipe()
    {
        if (isDestroying) return; 
        _ = InvalidateRecipeAsync();
    }
    
    private async Task InvalidateRecipeAsync()
    {
        CancelCurrentAnimation();
        isAnimatingScale = true;

        Vector3 startScale = rectTransform.localScale;
        Vector3 peakScale = Vector3.one * (popInvalidateAnimationScaleMultiplier * targetScale);

        try
        {
            scaleTweenHandle = LMotion.Create(startScale, peakScale, popAnimationSemiDuration)
                .WithEase(Ease.OutQuad) 
                .WithLoops(2, LoopType.Yoyo) 
                .BindToLocalScale(rectTransform);

            await scaleTweenHandle;

            isAnimatingScale = false; 
        }
        catch (OperationCanceledException) { }
    }
}