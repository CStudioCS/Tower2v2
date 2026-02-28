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
    
    private RecipeSlot targetSlot;
    private RecipeSlot TargetSlot
    {
        get => targetSlot;
        set
        {
            if (targetSlot)
                targetSlot.ResolutionChanged -= OnResolutionChanged;
            if (value)
                value.ResolutionChanged += OnResolutionChanged;
            targetSlot = value;
        }
    }
    private Vector2 TargetPosition => TargetSlot.RecipePosition;
    private bool overrideTargetScaleTo0;
    private float TargetScale => overrideTargetScaleTo0 ? 0 : TargetSlot.RecipeScale;

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
            TargetSlot = slot;
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
        TargetSlot = slot;
        overrideTargetScaleTo0 = false;
    }

    private void SetPosition(Vector2 position) => rectTransform.position = position;
    private void SetScale(float scale) => rectTransform.localScale = scale * Vector3.one;

    private void ReachTargetPosition() => SetPosition(TargetPosition);
    private void ReachTargetScale() => SetScale(TargetScale);
    private void ReachTarget() { ReachTargetPosition(); ReachTargetScale(); }

    public void Disappear(bool animate = false)
    {
        if (isDestroying) return;
        isDestroying = true;
        
        if (animate)
        {
            overrideTargetScaleTo0 = true;
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
        SetPosition(Vector2.SmoothDamp(rectTransform.position, TargetPosition, ref velocity, transitionTime));
        
        if (!isAnimatingScale)
        {
            SetScale(Mathf.SmoothDamp(rectTransform.localScale.x, TargetScale, ref scaleVelocity, transitionTime));
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
        Vector3 peakScale = Vector3.one * (popValidateAnimationScaleMultiplier * TargetScale);

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
        Vector3 peakScale = Vector3.one * (popInvalidateAnimationScaleMultiplier * TargetScale);

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

    private void OnResolutionChanged()
    {
        velocity = Vector2.zero;
        scaleVelocity = 0f;
        ReachTarget();
    }

    private void OnDisable()
    {
        TargetSlot = null;
    }
}