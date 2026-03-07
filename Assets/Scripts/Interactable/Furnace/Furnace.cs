using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class Furnace : Interactable
{
    [Header("Furnace")]
    [SerializeField] private float cookTime = 4;
    [SerializeField] private Item brickItemPrefab;
    [SerializeField] private ProgressBar progressBar;

    private PlayerTeam.Team itemCookedByTeam;

    private State state;
    public State FurnaceState => state;
    public enum State { Empty, Cooking, Cooked }

    public event Action StartedCooking;
    public event Action StoppedCooking;

    public override bool CanInteract(Player player)
    {
        if (!LevelManager.InGame)
            return false;
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
                SoundManager.instance.PlaySound("FurnaceBricks");
                player.GrabNewItem(brickItemPrefab, itemCookedByTeam);
                player.PlayerStats.bricksCooked++;
                state = State.Empty;
                progressBar.ResetProgress();
                break;
        }
    }

    public override float GetInteractionTime() => 0;


    public IEnumerator Cook()
    {
        StartedCooking?.Invoke();
        int index = SoundManager.instance.PlaySound("FurnaceFire");

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

        SoundManager.instance.StopSound(index);
        StopCooking();
    }
    
    protected override void OnGameEnded()
    {
        base.OnGameEnded();
        state = State.Empty;
        StopCooking();
    }

    private void StopCooking()
    {
        StoppedCooking?.Invoke();
    }
}
