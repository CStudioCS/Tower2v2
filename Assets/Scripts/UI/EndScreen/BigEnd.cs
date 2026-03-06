using System.Collections;
using UnityEngine;

public class BigEnd : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public IEnumerator Display()
    {
        animator.SetTrigger("Display");

        yield return null; //wait one frame for animator to change state (kinda ghetto)
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        //the yield returns here are to make the EndScreen global animation wait for the big end screen to finish it's animation
        //before displaying the tower card
    }
}
