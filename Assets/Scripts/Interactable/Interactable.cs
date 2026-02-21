using UnityEngine;

/// <summary>
/// An Interactable is everything a player interacts with. When a player is standing within the bounds of the
/// trigger collider and if CanInteract(player) is evaluated to true, the player can call the Interact function of the Interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{        
    public bool IsAlreadyInteractedWith { get; set; }
    private bool isHighlighted = false;

    public virtual void Interact(Player player) { }
    public abstract float GetInteractionTime();

    public virtual bool CanInteract(Player player) => false;

    [SerializeField] private float outlineThickness = 1f;
    [SerializeField] private Color outlineColor = Color.white;

    private SpriteRenderer _spriteRenderer;
    private MaterialPropertyBlock _propBlock;

    void Awake()
    {
         if (!TryGetComponent<SpriteRenderer>(out _spriteRenderer))
            Debug.LogError("Interactable " + gameObject.name + " does not have a SpriteRenderer component.");
           
        _propBlock = new MaterialPropertyBlock();
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
        {
            player.insideInteractableList.Remove(this);

            if(isHighlighted)
                Highlight(false);
        }
    }
    
    private void Start()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameEnded += OnGameEnded;
    }

    public void Highlight(bool highlighted)
    {
        if (isHighlighted == highlighted) 
            return;
        
        isHighlighted = highlighted;

        if (_spriteRenderer == null)
            return;

        _spriteRenderer.GetPropertyBlock(_propBlock);

        if (isHighlighted)
        {
            _propBlock.SetFloat("_OutlineSize", outlineThickness);
            _propBlock.SetColor("_OutlineColor", outlineColor);
            Debug.Log("Highlighting " + gameObject.name);
        }
        else
        {
            _propBlock.SetFloat("_OutlineSize", 0f);
        }

        _spriteRenderer.SetPropertyBlock(_propBlock);
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
