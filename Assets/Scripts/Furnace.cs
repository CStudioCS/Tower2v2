using System.Collections;
using UnityEngine;

public class Furnace : Interactable
{
    [SerializeField] private float cookTime;
    [SerializeField] private RectTransform ProgressBarFill;
    [SerializeField] private GameObject brickPiecePrefab;

    private State state;
    private float maxProgressBarFill;
    private enum State { Empty, Cooking, Cooked }

    private void Awake()
    {
        maxProgressBarFill = ProgressBarFill.sizeDelta.x;
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
        if(state == State.Empty)
        {
            StartCoroutine(Cook());

            player.ConsumeCurrentItem();
        }
        else if(state == State.Cooked)
        {
            player.isHolding = true;
            player.heldItem = Instantiate(brickPiecePrefab, player.transform).GetComponent<Item>();
            state = State.Empty;
            ResetProgressBar();
        }
    }

    private IEnumerator Cook()
    {
        state = State.Cooking;

        ProgressBarFill.parent.gameObject.SetActive(true);
        float t = 0;

        while (t < cookTime)
        {
            ProgressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, maxProgressBarFill, t / cookTime), ProgressBarFill.sizeDelta.y);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        ProgressBarFill.sizeDelta = new Vector2(maxProgressBarFill, ProgressBarFill.sizeDelta.y);
        state = State.Cooked;
    }

    private void ResetProgressBar()
    {
        ProgressBarFill.parent.gameObject.SetActive(false);
        ProgressBarFill.sizeDelta = new Vector2(0, ProgressBarFill.sizeDelta.y);
    }
}
