using UnityEngine;

/// <summary>
/// An Interactable is everything a player interacts with. When a player is standing within the bounds of the
/// trigger collider and if CanInteract(player) is evaluated to true, the player can call the Interact function of the Interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");
    public bool IsAlreadyInteractedWith { get; set; }
    private int highlightedPlayerCount = 0;

    [SerializeField] protected SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => spriteRenderer;

    private MaterialPropertyBlock propBlock;

    public virtual void Interact(Player player) { }
    public abstract float GetInteractionTime();

    public virtual bool CanInteract(Player player) => false;

    protected virtual void Awake()
    {
        InitializeHighlight();
    }

    protected void InitializeHighlight()
    {
        if (propBlock != null) 
            return;

        if (spriteRenderer == null)
            return;

        propBlock = new MaterialPropertyBlock();
    }

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

    public virtual void TryHighlight(bool highlighted, Player player)
    {
        if (!CheckIfCanBeHighlighted(player) && highlighted)
            return;

        if (highlighted) 
            highlightedPlayerCount++;
        else 
            highlightedPlayerCount--;

        if (!highlighted && highlightedPlayerCount > 0) 
            return;

        if (highlighted && highlightedPlayerCount >= 2) 
            return;

        Highlight(highlighted);
    }

    private void Highlight(bool highlighted = true)
    {
        if (spriteRenderer == null || propBlock == null)
            return;
            
        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(OutlineEnabled, highlighted? 1f: 0f);
        spriteRenderer.SetPropertyBlock(propBlock);
    }
    
    protected virtual void OnGameAboutToStart()
    {
        IsAlreadyInteractedWith = false;
    }
    public virtual bool CheckIfCanBeHighlighted(Player player) => spriteRenderer != null && propBlock != null;

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
