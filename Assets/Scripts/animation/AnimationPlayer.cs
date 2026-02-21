using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    public void StartCrafting()
    {
        animator.SetBool("is_crafting", true);
    }
    public void EndCrafting()
    {
        animator.SetBool("is_crafting", false);
    }

    public void Grab()
    {
        animator.SetTrigger("grab");
    }

    public void Drop()
    {
        animator.SetTrigger("drop");
    }


    void Update()
    {
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }
}
