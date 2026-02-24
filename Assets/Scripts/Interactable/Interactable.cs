using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Interactable is everything a player interacts with. When a player is standing within the bounds of the
/// trigger collider and if CanInteract(player) is evaluated to true, the player can call the Interact function of the Interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{        
    public bool IsAlreadyInteractedWith { get; set; }
    private int highlightedPlayerCount = 0;

    [Header("Highlight Settings")]
    [SerializeField] private float outlineThickness = 5f;
    [SerializeField] private Color outlineColor = Color.white;

    [SerializeField] protected SpriteRenderer spriteRenderer;
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

    public virtual void Highlight(bool highlighted)
    {
        if (highlighted) 
            highlightedPlayerCount++;
        else 
            highlightedPlayerCount--;

        if (!highlighted && highlightedPlayerCount > 0) 
            return;

        if (highlighted && highlightedPlayerCount >= 2) 
            return;

        if (spriteRenderer == null || propBlock == null)
            return;

        spriteRenderer.GetPropertyBlock(propBlock);

        if (highlighted)
        {
            propBlock.SetFloat("_OutlineSize", outlineThickness);
            propBlock.SetColor("_OutlineColor", outlineColor);
        }
        else
        {
            propBlock.SetFloat("_OutlineSize", 0f);
        }

        spriteRenderer.SetPropertyBlock(propBlock);
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
