using System.ComponentModel;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public float interactionTime;

    public abstract void Interact(Player player);
    public virtual bool CanInteract(Player player) => true;

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
