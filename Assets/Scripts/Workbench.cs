using UnityEngine;
using System.Collections;
public class Workbench : Interactable
{
    [SerializeField] private int craftTime;
    [SerializeField] private RectTransform ProgressBarFill;
    [SerializeField] private GameObject woodCraftedPrefab;
    private State state;
    
    private float maxProgressBarFill;

    enum State { Empty, Crafted, AsWood}

    public override bool CanInteract(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.heldItem == Player.HeldItem.Wood;
            case State.AsWood:
                return player.heldItem == Player.HeldItem.Nothing;
            case State.Crafted:
                return player.heldItem == Player.HeldItem.Nothing;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    public override void Interact(Player player)
    {
        if (state == State.Empty)
        {
            state = State.AsWood;
            GetComponent<SpriteRenderer>().color = Color.red;
            player.heldItem = Player.HeldItem.Nothing;
            Destroy(player.heldItemGameobject);
            player.heldItemGameobject = null;
        }
        else if (state == State.Crafted)
        {
            player.heldItem = Player.HeldItem.WoodCrafted;
            state = State.Empty;

            GetComponent<SpriteRenderer>().color = Color.white;
            // Assume tablePrefab is defined elsewhere
            player.heldItemGameobject = Instantiate(woodCraftedPrefab, player.transform);
            state = State.Empty;
        }
    }

    public override void InteractA(Player player)
    {
        if( state == State.AsWood)
        {
            state = State.Crafted;
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }

}
