using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Logo : MonoBehaviour
{
    private static readonly int Active = Animator.StringToHash("active");
    [SerializeField] private Image LogoImg;
    [SerializeField] private Animator animator;
    private bool droppedDown;

    private void Update()
    {
        if (InputUtility.AnyInputPressed && !droppedDown)
            StartCoroutine(Dropdown());
    }

    private IEnumerator Dropdown()
    {
        droppedDown = true;

        animator.SetBool(Active, false);

        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorClipInfo(0).Length);

        gameObject.SetActive(false);
    }
}
