using System.Collections;
using UnityEngine;

public class Furnace : Interactable
{
    [SerializeField] private float cookTime;
    [SerializeField] private Item brickItemPrefab;
    [SerializeField] private ProgressBar progressBar;

    private State state;
    private float maxProgressBarFill;
    private enum State { Empty, Cooking, Cooked }

    protected override bool CanInteractPrimary(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.itemType == Resources.Type.Clay;
            case State.Cooking:
                return false;
            case State.Cooked:
                return !player.IsHolding;
            default:
                throw new UnityException("Furnace state not handled in CanInteract");
        }
    }

    protected override void InteractPrimary(Player player)
    {
        if (state == State.Empty)
        {
            StartCoroutine(Cook());

            player.ConsumeCurrentItem();
        }
        else if (state == State.Cooked)
        {
            player.GrabNewItem(brickItemPrefab);
            state = State.Empty;
            progressBar.ResetProgress();
        }
    }

    private IEnumerator Cook()
    {
        state = State.Cooking;

        progressBar.StartProgress();
        float t = 0;

        while (t < cookTime)
        {
            progressBar.UpdateProgress(t / cookTime);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }
        progressBar.SetProgressMax();
        state = State.Cooked;
    }
}
