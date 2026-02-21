using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    public void StartCutting()
    {
        animator.SetBool("is_cutting", true);
    }
    public void EndCutting()
    {
        animator.SetBool("is_cutting", false);
    }

    public void StartCollecting()
    {
        animator.SetBool("is_collecting", true);
    }
    public void EndCollecting()
    {
        animator.SetBool("is_collecting", false);
    }

    public void HasItem(bool hasItem)
    {
        animator.SetBool("has_item", hasItem);
    }

    public void Grab()
    {
        animator.SetTrigger("pick_up");
        animator.SetBool("has_item", true);
    }
    public void Drop()
    {
        animator.SetTrigger("drop");
        animator.SetBool("has_item", false);
    }


    void Update()
    {
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }
}
