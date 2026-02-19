using UnityEngine;

/// <summary>
/// An Interactable is everything a player interacts with. When a player is standing within the bounds of the
/// trigger collider and if CanInteract(player) is evaluated to true, the player can call the Interact function of the Interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{        
    public bool IsAlreadyInteractedWith { get; set; }

    public virtual void Interact(Player player) { }
    public abstract float GetInteractionTime();

    public virtual bool CanInteract(Player player) => false;

    // When the player walks inside the interactable, we tell it that it is inside
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player))
            player.insideInteractableList.Add(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player) && player.insideInteractableList.Contains(this))
            player.insideInteractableList.Remove(this);
    }
    
    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEnded += OnGameEnded;
    }

    public void Highlight(bool highlighted)
    {
        //à remplir par la PR de highlight
    }
    protected virtual void OnGameAboutToStart()
    {
        IsAlreadyInteractedWith = false;
    }
    
    protected virtual void OnGameEnded()
    {
        IsAlreadyInteractedWith = false;
    }
    
    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }
}
