using UnityEngine;

/// <summary>
/// An Interactable is everything a player interacts with. When a player is standing within the bounds of the
/// trigger collider and if CanInteract(player) is evaluated to true, the player can call the Interact function of the Interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");
    public bool IsAlreadyInteractedWith { get; set; }
    private int highlightedPlayerCount;

    [SerializeField] protected SpriteRenderer[] spriteRenderers;

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

        if (spriteRenderers?.Length == 0)
            return;

        propBlock = new MaterialPropertyBlock();
    }

    // When the player walks inside the interactable, we tell it that it is inside
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
            player.insideInteractableList.Add(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player) && player.insideInteractableList.Contains(this))
            player.insideInteractableList.Remove(this);
    }
    
    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEndedOrReturnedToLobby += OnGameEndedOrReturnedToLobby;
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
        spriteRenderers[0].GetPropertyBlock(propBlock);
        propBlock.SetFloat(OutlineEnabled, highlighted? 1f: 0f);
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.SetPropertyBlock(propBlock);   
        }
    }
    
    protected virtual void OnGameAboutToStart()
    {
        IsAlreadyInteractedWith = false;
    }

    public virtual bool CheckIfCanBeHighlighted(Player player) => propBlock != null && spriteRenderers?.Length > 0;

    protected virtual void OnGameEndedOrReturnedToLobby()
    {
        IsAlreadyInteractedWith = false;
    }
    
    private void OnDisable()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.Instance.GameEndedOrReturnedToLobby -= OnGameEndedOrReturnedToLobby;
    }
}
