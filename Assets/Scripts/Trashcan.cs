using UnityEngine;

public class Trashcan : Interactable
{   
    public override void Interact(Player player)
    {
        player.heldItem = Player.HeldItem.Nothing;
        if (player.heldItemGameobject != null)
            Destroy(player.heldItemGameobject);
        player.heldItemGameobject = null;
    }

    public override bool CanInteract(Player player) {
        if (player.heldItem != Player.HeldItem.Nothing) {
                return true;
        }
        else
        {
            return false;
        }    
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
