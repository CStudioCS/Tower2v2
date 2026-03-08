using System;
using UnityEditor.Animations;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public Animator Animator => animator;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private string isCuttingId;
    [SerializeField] private string isCollectingId;
    [SerializeField] private string hasItemId;
    [SerializeField] private string dropTriggerId;
    [SerializeField] private string speedId;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AnimatorOverrideController animatorOverrideController;
    [SerializeField] private AnimatorController animatorController;
    [SerializeField] private PlayerTeam playerTeam;
    [SerializeField] private Player player;

    private void OnEnable()
    {
        playerTeam.TeamChanged += OnTeamChanged;
        OnTeamChanged();
    }

    private void OnTeamChanged()
    {
        if (playerTeam.CurrentTeam == PlayerTeam.Team.Left)
            animator.runtimeAnimatorController = animatorController;
        else
            animator.runtimeAnimatorController = animatorOverrideController;
    }

    public void StartCutting()
        => animator.SetBool(isCuttingId, true);

    public void StartCollecting()
        => animator.SetBool(isCollectingId, true);

    public void EndInteraction()
    {
        animator.SetBool(isCuttingId, false);
        animator.SetBool(isCollectingId, false);
    }

    public void HasItem(bool hasItem)
        => animator.SetBool(hasItemId, hasItem);

    public void Grab()
        => animator.SetBool(hasItemId, true);

    public void Drop()
    {
        animator.SetTrigger(dropTriggerId);
        animator.SetBool(hasItemId, false);
    }


    void Update()
    {
        animator.SetFloat(speedId, rb.linearVelocity.magnitude);
        if (rb.linearVelocity.x > 0.1f)
        {
            spriteRenderer.flipX = true;
        }
        else if (rb.linearVelocity.x < -0.1f)
        {
            spriteRenderer.flipX = false;
        }

        FlipHeldItem();
    }

    void FlipHeldItem()
    {
        if (player.IsHolding)
        {
            player.HeldItem.SpriteRenderer.flipX = spriteRenderer.flipX;
        }
    }

    private void OnDisable()
    {
        playerTeam.TeamChanged -= OnTeamChanged;
    }
}
