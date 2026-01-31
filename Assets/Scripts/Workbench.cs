using UnityEngine;

public class Workbench : Interactable
{
    [SerializeField] private GameObject woodPlankItemPrefab;
    
    private State state;
    private enum State { Empty, HasWoodLog, HasWoodPlank}

    public override bool CanInteract(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.isHolding && player.heldItem.itemType == Resources.Type.WoodLog;
            case State.HasWoodLog:
                return false;
            case State.HasWoodPlank:
                return !player.isHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    public override bool CanInteractA(Player player)
        => state == State.HasWoodLog && !player.isHolding;

    public override void Interact(Player player)
    {
        if (state == State.Empty)
        {
            state = State.HasWoodLog;

            player.ConsumeCurrentItem();

            GetComponent<SpriteRenderer>().color = Color.red;
        }
        else if (state == State.HasWoodPlank) //Could just write else here, but ig this is clearer
        {
            state = State.Empty;

            player.isHolding = true;
            player.heldItem = Instantiate(woodPlankItemPrefab, player.transform).GetComponent<Item>();

            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    public override void InteractA(Player player)
    {
        state = State.HasWoodPlank;
        GetComponent<SpriteRenderer>().color = Color.blue;
    }

}