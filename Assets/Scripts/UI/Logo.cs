using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Logo : MonoBehaviour
{
    [SerializeField] private Image LogoImg;
    [SerializeField] private Animator animator;
    private bool droppedDown;


    private void Update()
    {
        if (Input.anyKey && !droppedDown)
            StartCoroutine(Dropdown());
    }

    private IEnumerator Dropdown()
    {
        droppedDown = true;

        animator.SetBool("active", false);

        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0).Length);

        gameObject.SetActive(false);
    }
}
