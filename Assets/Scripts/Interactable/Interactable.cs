using UnityEngine;

/// <summary>
/// An Interactable is everything a player interacts with. When a player is standing within the bounds of the
/// trigger collider and if CanInteract(player) is evaluated to true, the player can call the Interact function of the Interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    public enum InteractionType { Primary, Secondary }
    
    [Header("Interaction Times")]
    [SerializeField] protected float interactionTime;
    protected float interactionTimeSecondary; //unused
    
    public bool IsAlreadyInteractedWith { get; set; }
    private bool isHighlighted = false;

    public virtual void Interact(InteractionType type, Player player)
    {
        switch (type)
        {
            case InteractionType.Primary: InteractPrimary(player); break;
            case InteractionType.Secondary: InteractSecondary(player); break;
        }
    }

    protected virtual void InteractPrimary(Player player) { }
    protected virtual void InteractSecondary(Player player) { }

    public float GetInteractionTime(InteractionType type) => type == InteractionType.Primary ? interactionTime : interactionTimeSecondary;

    public virtual bool CanInteract(InteractionType type, Player player)
    {
        switch (type)
        {
            case InteractionType.Primary: return CanInteractPrimary(player);
            case InteractionType.Secondary: return CanInteractSecondary(player);
        }
        return false;
    }

    protected virtual bool CanInteractPrimary(Player player) => false;
    protected virtual bool CanInteractSecondary(Player player) => false;

    // When the player walks inside the interactable, we tell it that it is inside
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player))
            player.insideInteractableList.Add(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player player) && player.insideInteractableList.Contains(this))
        {
            this.Highlight(false);
            player.insideInteractableList.Remove(this);
        }
    }
    
    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEnded += OnGameEnded;
    }

    public virtual void Highlight(bool highlighted)
    {
        if (isHighlighted == highlighted) 
            return;
        
        isHighlighted = highlighted;
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
