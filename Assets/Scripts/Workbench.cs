using UnityEngine;

public class Workbench : Interactable
{
    [SerializeField] private int craftTime;
    [SerializeField] private RectTransform ProgressBarFill;
    [SerializeField] private GameObject woodCraftedPrefab;
    private State state;
    
    private float maxProgressBarFill;

    enum State { Empty, Crafted, HasWoodLog}

    public override bool CanInteract(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.isHolding && player.heldItem == Resources.Type.WoodLog;
            case State.HasWoodLog:
                return !player.isHolding;
            case State.Crafted:
                return !player.isHolding;
            default:
                throw new UnityException("Workbench state not handled in CanInteract");
        }
    }

    public override void Interact(Player player)
    {
        if (state == State.Empty)
        {
            state = State.HasWoodLog;
            GetComponent<SpriteRenderer>().color = Color.red;
            player.isHolding = false;
            Destroy(player.heldItemGameobject);
            player.heldItemGameobject = null;
        }
        else if (state == State.Crafted)
        {
            player.isHolding = true;
            player.heldItem = Resources.Type.WoodPlank;
            state = State.Empty;

            GetComponent<SpriteRenderer>().color = Color.white;
            // Assume tablePrefab is defined elsewhere
            player.heldItemGameobject = Instantiate(woodCraftedPrefab, player.transform);
            state = State.Empty;
        }
    }

    public override void InteractA(Player player)
    {
        if( state == State.HasWoodLog)
        {
            state = State.Crafted;
            GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }

}
