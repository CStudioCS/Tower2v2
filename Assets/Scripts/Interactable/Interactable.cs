using UnityEngine;

/// <summary>
/// An Interactable is everything a player interacts with. When a player is standing within the bounds of the
/// trigger collider and if CanInteract(player) is evaluated to true, the player can call the Interact function of the Interactable.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    public enum ExecutionTarget { ServerSide, ClientSide }

    [Header("Network Settings")]
    [Tooltip("ClientSide for UI - ServerSide for Gameplay")]
    public ExecutionTarget executionTarget = ExecutionTarget.ServerSide;

    [SerializeField] private string uniqueId;
    public int NetworkId { get; private set; }

    private static readonly int OutlineEnabled = Shader.PropertyToID("_OutlineEnabled");

    [SerializeField] protected SpriteRenderer[] spriteRenderers;

    private MaterialPropertyBlock propBlock;

    public virtual void Interact(Player player) { }
    public abstract float GetInteractionTime();

    public virtual bool CanInteract(Player player) => false;

    protected virtual void Awake()
    {
        InitializeHighlight();

        if (!string.IsNullOrEmpty(uniqueId))
            RegisterNetworkId(Animator.StringToHash(uniqueId));
    }

    public void RegisterNetworkId(int fusionNativeId)
    {
        NetworkId = fusionNativeId;
        InteractableRegistry.Register(this);
    }

#if UNITY_EDITOR
    protected void OnValidate()
    {
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = System.Guid.NewGuid().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

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
        {
            if (player.Object?.IsValid != true) 
                return;

            if (player.HasStateAuthority)
                player.LocalEnterInteractable(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Player player))
        {
            if (player.Object?.IsValid != true)
                return;

            if (player.HasStateAuthority)
                player.LocalExitInteractable(this);
        }
    }
    
    private void Start()
    {
        LevelManager.GameAboutToStart += OnGameAboutToStart;
        LevelManager.ReturnedToLobby += OnReturnedToLobby;
        LevelManager.GameEnded += OnGameEnded;
    }

    public virtual void RefreshHighlight()
    {
        if (propBlock == null || (spriteRenderers?.Length ?? 0) == 0)
            return;

        bool shouldHighlight = false;

        foreach (Player p in PlayerRegistry.All)
        {
            if (p.SyncTargetId == NetworkId && CheckIfCanBeHighlighted(p))
            {
                shouldHighlight = true;
                break;
            }
        }

        Highlight(shouldHighlight);
    }

    public bool IsAlreadyInteractedWith()
    {
        foreach (Player p in PlayerRegistry.All)
        {
            if (p.SyncIsInteracting && p.SyncTargetId == NetworkId)
                return true;
        }
        return false;
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
    
    protected virtual void OnGameAboutToStart() { }

    public virtual void OnTargetedBy(Player player, bool targeted) { }

    public virtual bool CheckIfCanBeHighlighted(Player player) => propBlock != null && spriteRenderers?.Length > 0;

    protected virtual void OnReturnedToLobby() { }

    protected virtual void OnGameEnded() { }

    private void OnDisable()
    {
        LevelManager.GameAboutToStart -= OnGameAboutToStart;
        LevelManager.ReturnedToLobby -= OnReturnedToLobby;
        LevelManager.GameEnded -= OnGameEnded;
    }

    protected virtual void OnDestroy() => InteractableRegistry.Unregister(this);
}
