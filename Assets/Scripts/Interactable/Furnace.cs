using System.Collections;
using UnityEngine;

public class Furnace : Interactable
{
    [SerializeField] private float cookTime = 4;
    [SerializeField] private Item brickItemPrefab;
    [SerializeField] private ProgressBar progressBar;

    private State state;
    private enum State { Empty, Cooking, Cooked }

    protected override bool CanInteractPrimary(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.itemType == Item.Type.Clay;
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
        switch (state)
        {
            case State.Empty:
                StartCoroutine(Cook());
                player.ConsumeCurrentItem();
                break;
            case State.Cooked:
                player.GrabNewItem(brickItemPrefab);
                state = State.Empty;
                progressBar.ResetProgress();
                break;
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

            t += Time.deltaTime;
            yield return null;
        }
        progressBar.SetProgressMax();
        state = State.Cooked;
    }
}
