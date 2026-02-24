using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class Furnace : Interactable
{
    [Header("Furnace")]
    [SerializeField] private float cookTime = 4;
    [SerializeField] private Item brickItemPrefab;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private AudioSource audioSourceFire;
    [SerializeField] private AudioSource audioSourceBricks;

    private PlayerTeam.Team itemCookedByTeam;

    private State state;
    private enum State { Empty, Cooking, Cooked }

    public override bool CanInteract(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.ItemType == Item.Type.Clay;
            case State.Cooking:
                return false;
            case State.Cooked:
                return !player.IsHolding;
            default:
                throw new UnityException("Furnace state not handled in CanInteract");
        }
    }

    public override void Interact(Player player)
    {
        switch (state)
        {
            case State.Empty:
                StartCoroutine(Cook());
                player.ConsumeCurrentItem();
                itemCookedByTeam = player.PlayerTeam.CurrentTeam;
                break;
            case State.Cooked:
                audioSourceBricks.Play();

                player.GrabNewItem(brickItemPrefab, itemCookedByTeam);
                player.PlayerStats.bricksCooked++;
                state = State.Empty;
                progressBar.ResetProgress();
                break;
        }
    }

    public override float GetInteractionTime() => 0;

    private IEnumerator Cook()
    {
        audioSourceFire.Play();

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

        audioSourceFire.Stop();
    }
    
    protected override void OnGameEnded()
    {
        base.OnGameEnded();
        state = State.Empty;
    }
}
