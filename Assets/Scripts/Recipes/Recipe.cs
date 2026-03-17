using System;
using System.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

public class Recipe : MonoBehaviour
{
    [SerializeField] private Item.Type type;
    public Item.Type Type => type;
    [SerializeField] private Transform itemGraphicsTransform;
    
    [SerializeField] private float transitionTime = .1f;
    [SerializeField] private float popAnimationSemiDuration = .2f;
    [SerializeField] private float popValidateAnimationScaleMultiplier = 1.5f;
    [SerializeField] private float popInvalidateAnimationScaleMultiplier = 1.5f;
    
    [SerializeField] private float delayBeforeMove = 0.3f;

    private RecipeSlot targetSlot;
    private Vector2 TargetPosition => targetSlot.RecipePosition;
    private bool overrideTargetScaleTo0;
    private float TargetScale => overrideTargetScaleTo0 ? 0 : targetSlot.RecipeScale;

    private Vector2 velocity;
    private float scaleVelocity;

    private MotionHandle scaleTweenHandle;
    private bool isAnimatingScale;
    private bool isDestroying;
    
    private int currentMoveCommandId;

    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private SpriteRenderer bannerSpriteRenderer;

    public void Appear(RecipeSlot slot, bool animate = false)
    {
        if (animate)
        {
            SetScale(0);
            targetSlot = slot;
            overrideTargetScaleTo0 = true;
            ReachTargetPosition();
            MoveToRecipeSlot(slot);
        }
        else
        {
            SetSlotAsTarget(slot);
            ReachTarget();
        }
    }

    public void MoveToRecipeSlot(RecipeSlot slot, bool deadRecipeSlot = false)
    {
        currentMoveCommandId++;
        _ = MoveToRecipeSlotAsync(slot, currentMoveCommandId, deadRecipeSlot);
    }

    private async Task MoveToRecipeSlotAsync(RecipeSlot slot, int commandId, bool deadRecipeSlot = false)
    {
        if (delayBeforeMove > 0f)
        {
            await Task.Delay(Mathf.RoundToInt(delayBeforeMove * 1000f));
            if (this == null || gameObject == null) return;
            if (currentMoveCommandId != commandId) return;
        }

        if (deadRecipeSlot)
        {
            SetSpriteRenderersMaskInteraction(SpriteMaskInteraction.VisibleOutsideMask);
            Destroy(gameObject, transitionTime * 10);
        }

        SetSlotAsTarget(slot);
    }

    private void SetSlotAsTarget(RecipeSlot slot)
    {
        targetSlot = slot;
        overrideTargetScaleTo0 = false;
    }

    private void SetPosition(Vector2 position) => transform.localPosition = position;
    private void SetScale(float scale) => transform.localScale = scale * Vector3.one;

    private void ReachTargetPosition() => SetPosition(TargetPosition);
    private void ReachTargetScale() => SetScale(TargetScale);

    private void ReachTarget()
    {
        ReachTargetPosition(); ReachTargetScale();
    }

    private void Update()
    {
        if (targetSlot == null) return;
        
        SetPosition(Vector2.SmoothDamp(transform.localPosition, TargetPosition, ref velocity, transitionTime));
        
        if (!isAnimatingScale)
        {
            SetScale(Mathf.SmoothDamp(transform.localScale.x, TargetScale, ref scaleVelocity, transitionTime));
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
    
    public void ValidateRecipe(RecipeSlot deadRecipeSlot)
    {
        if (isDestroying) return; 
        isDestroying = true;
        MoveToRecipeSlot(deadRecipeSlot, true);
        _ = ValidateRecipeAsync();
    }

    private async Task ValidateRecipeAsync()
    {
        CancelCurrentAnimation();
        isAnimatingScale = true;

        Vector3 startScale = itemGraphicsTransform.localScale;
        Vector3 peakScale = Vector3.one * (popValidateAnimationScaleMultiplier * TargetScale);

        try
        {
            scaleTweenHandle = LMotion.Create(startScale, peakScale, popAnimationSemiDuration)
                .WithEase(Ease.OutQuad)
                .BindToLocalScale(itemGraphicsTransform);

            await scaleTweenHandle;

            scaleTweenHandle = LMotion.Create(peakScale, Vector3.zero, popAnimationSemiDuration)
                .WithEase(Ease.InQuad)
                .BindToLocalScale(itemGraphicsTransform);

            await scaleTweenHandle;
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

        Vector3 startScale = itemGraphicsTransform.localScale;
        Vector3 peakScale = Vector3.one * (popInvalidateAnimationScaleMultiplier * TargetScale);

        try
        {
            scaleTweenHandle = LMotion.Create(startScale, peakScale, popAnimationSemiDuration)
                .WithEase(Ease.OutQuad) 
                .WithLoops(2, LoopType.Yoyo) 
                .BindToLocalScale(itemGraphicsTransform);

            await scaleTweenHandle;

            isAnimatingScale = false; 
        }
        catch (OperationCanceledException) { }
    }

    public void SetSpriteRenderersMaskInteraction(SpriteMaskInteraction maskInteraction = SpriteMaskInteraction.None)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.maskInteraction = maskInteraction;
        }
    }

    public void SetBannerSortOrder(int sortOrder)
    {
        bannerSpriteRenderer.sortingOrder = sortOrder;
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {   
            spriteRenderer.sortingOrder = 10 + sortOrder;
        }
    }

    public void IncrementBannerSortOrder() => SetBannerSortOrder(bannerSpriteRenderer.sortingOrder + 1);

    private void OnDisable()
    {
        targetSlot = null;
    }
}