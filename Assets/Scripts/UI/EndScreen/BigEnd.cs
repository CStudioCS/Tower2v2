using UnityEngine;

public class BigEnd : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public void Display()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Display");
    }
}
