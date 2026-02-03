using System.Collections;
using UnityEngine;

public class Furnace : Interactable
{
    [SerializeField] private float cookTime;
    [SerializeField] private RectTransform progressBarFill;
    [SerializeField] private Item brickItem;

    private State state;
    private float maxProgressBarFill;
    private enum State { Empty, Cooking, Cooked }

    private void Awake()
    {
        maxProgressBarFill = progressBarFill.sizeDelta.x;
        ResetProgressBar();
    }

    public override bool CanInteract(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.isHolding && player.heldItem.itemType == Resources.Type.Clay;
            case State.Cooking:
                return false;
            case State.Cooked:
                return !player.isHolding;
            default:
                throw new UnityException("Furnace state not handled in CanInteract");
        }
    }

    public override void Interact(Player player)
    {
        if (state == State.Empty)
        {
            StartCoroutine(Cook());

            player.ConsumeCurrentItem();
        }
        else if (state == State.Cooked)
        {
            player.GrabNewItem(brickItem);
            state = State.Empty;
            ResetProgressBar();
        }
    }

    private IEnumerator Cook()
    {
        state = State.Cooking;

        progressBarFill.parent.gameObject.SetActive(true);
        float t = 0;

        while (t < cookTime)
        {
            progressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, maxProgressBarFill, t / cookTime), progressBarFill.sizeDelta.y);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        progressBarFill.sizeDelta = new Vector2(maxProgressBarFill, progressBarFill.sizeDelta.y);
        state = State.Cooked;
    }

    private void ResetProgressBar()
    {
        progressBarFill.parent.gameObject.SetActive(false);
        progressBarFill.sizeDelta = new Vector2(0, progressBarFill.sizeDelta.y);
    }
}
