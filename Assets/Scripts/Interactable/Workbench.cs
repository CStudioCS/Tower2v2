using UnityEngine;

public class Workbench : Interactable
{
    private State state;

    [SerializeField] private float putOrPickUpItemInteractionTime = 0f;
    [SerializeField] private float cutWoodInteractionTime = 1f;
    private float currentInteractionTime;

    [Header("References")]
    [SerializeField] private Item woodPlankItemPrefab;

    private PlayerTeam.Team cutLastByTeam;

    [SerializeField] private GameObject woodOnTable;
    [SerializeField] private GameObject woodPlanckOnTable;

    private enum State { Empty, HasWoodLog, HasWoodPlank }

    private int soundIndex = -1;

    public override bool CanInteract(Player player)
    {
        if (!LevelManager.InGame)
            return false;
        switch (state)
        {
            case State.Empty:
                return player.IsHolding && player.HeldItem.ItemType == Item.Type.WoodLog;
            case State.HasWoodLog:
                return !player.IsHolding;
            case State.HasWoodPlank:
                return !player.IsHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    public override void Interact(Player player)
    {
        switch (state)
        {
            case State.Empty:
                SoundManager.instance.PlaySound("WoodSound");

                state = State.HasWoodLog;
                player.ConsumeCurrentItem();
                currentInteractionTime = cutWoodInteractionTime;
                woodOnTable.SetActive(true);
                break;
            
            case State.HasWoodLog:
                state = State.HasWoodPlank;
                currentInteractionTime = putOrPickUpItemInteractionTime;
                player.PlayerStats.woodCut++;
                cutLastByTeam = player.PlayerTeam.CurrentTeam;
                woodOnTable.SetActive(false);
                woodPlanckOnTable.SetActive(true);
                break;
            
            case State.HasWoodPlank:
                SoundManager.instance.PlaySound("WoodSound");

                state = State.Empty;
                player.GrabNewItem(woodPlankItemPrefab, cutLastByTeam); //ownership for wood is determined by who cut it, not who collected it 
                woodPlanckOnTable.SetActive(false);
                break;
        }
    }

    public override float GetInteractionTime() => currentInteractionTime;
    
    protected override void OnGameEnded()
    {
        base.OnGameEnded();
        state = State.Empty;
        woodOnTable.SetActive(false);
        woodPlanckOnTable.SetActive(false);
    }

    private void Update()
    {
        if(IsAlreadyInteractedWith && soundIndex == -1)
        {
            soundIndex = SoundManager.instance.PlaySound("Hammer");
        }

        if (!IsAlreadyInteractedWith && soundIndex != -1)
        {
            SoundManager.instance.StopSound(soundIndex);
            soundIndex = -1;
        }
    }
}