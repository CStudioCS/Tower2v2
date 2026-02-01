using UnityEngine;

/// <summary>
/// An Interactable is everything a player interacts with. When a player is standing within the bounds of the
/// trigger collider and if CanInteract(player) is evaluated to true, the player can call the Interact function of the Interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    public float interactionTime;
    
    public float interactionTimeA;

    public abstract void Interact(Player player);
    
    public virtual void InteractA(Player player) {}
    public virtual bool CanInteract(Player player) => true;
    public virtual bool CanInteractA(Player player) => false;

    public bool isAlreadyInteractedWith=false;

    //When the player walks inside the interactable, we tell it that it is inside
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player))
            player.insideInteractable = this;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player) && player.insideInteractable == this)
            player.insideInteractable = null;
    }
}
