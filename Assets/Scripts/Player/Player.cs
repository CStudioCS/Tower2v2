using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color leftTeamColor;
    [SerializeField] private Color rightTeamColor;
    
    private Dictionary<LevelManager.Team, Color> teamColors;
    public Dictionary<LevelManager.Team, Color> TeamColors
    {
        get
        {
            if (teamColors == null)
            {
                teamColors = new Dictionary<LevelManager.Team, Color>
                {
                    { LevelManager.Team.Left, leftTeamColor },
                    { LevelManager.Team.Right, rightTeamColor }
                };
            }
            return teamColors;
        }
    }
    
    [HideInInspector] public Interactable insideInteractable;
    public bool IsHolding { get; private set; }
    public Item HeldItem { get; private set; }
    public bool Interacting { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerTeam playerTeam;
    public PlayerTeam PlayerTeam => playerTeam;
    [SerializeField] private PlayerControlBadge playerControlBadge;
    public PlayerControlBadge PlayerControlBadge => playerControlBadge;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ProgressBar progressBar;
    
    private InputAction interactAction;
    private InputAction cutWoodAction;

    private void Awake()
    {
        playerTeam.TeamChanged += OnTeamChanged;
        UpdateColor();
        interactAction = playerInput.actions.FindAction("Gameplay/Interact");
        cutWoodAction = playerInput.actions.FindAction("Gameplay/CutWood");
    }

    private void OnTeamChanged() => UpdateColor();
    private void UpdateColor()
    {
        spriteRenderer.color = TeamColors[playerTeam.CurrentTeam];
    }

    private void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            switch (LevelManager.Instance.GameState)
            {
                case LevelManager.State.Game:
                    StartInteracting(Interactable.InteractionType.Primary, interactAction);
                    break;
                case LevelManager.State.Lobby:
                    playerControlBadge.Interact();
                    break;
            }
        }

        if(cutWoodAction.WasPressedThisFrame())
        {
            switch (LevelManager.Instance.GameState)
            {
                case LevelManager.State.Game:
                    StartInteracting(Interactable.InteractionType.Secondary, cutWoodAction);
                    break;
            }
        }
    }

    private void StartInteracting(Interactable.InteractionType interactionType, InputAction inputAction)
    {
        // We check whether we're inside an interactable object and if yes if we can interact with it
        if (insideInteractable == null)
            return;
        if (insideInteractable.IsAlreadyInteractedWith) // I do not set isAlreadyInteractedWith to true in an else statement because it only applies to interactables with interaction time
            return;

        if (!insideInteractable.CanInteract(interactionType, this))
            return;

        float time = insideInteractable.GetInteractionTime(interactionType);
        
        // If we can interact instantly, we do it, else we need to wait for the interaction time
        if (time > 0)
        {
            insideInteractable.IsAlreadyInteractedWith = true;
            StartCoroutine(InteractTimer(interactionType, time, inputAction));
        }
        else
        {
            insideInteractable.Interact(interactionType, this);
        }
    }

    private IEnumerator InteractTimer(Interactable.InteractionType interactionType, float time, InputAction inputAction)
    {
        Interacting = true;
        progressBar.StartProgress();
        float t = 0;
        
        while(t < time)
        {
            // If at any point the player stops holding the interact button -> stop interacting
            if (!inputAction.IsPressed())
            {
                progressBar.ResetProgress();
                Interacting = false;
                insideInteractable.IsAlreadyInteractedWith = false;
                yield break;
            }

            progressBar.UpdateProgress(t / time);
            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        // We interacted with the object -> Reset everything and call the interact function
        Interacting = false;
        progressBar.ResetProgress();
        insideInteractable.Interact(interactionType, this);
        insideInteractable.IsAlreadyInteractedWith = false;
    }

    // Discards the item currently held
    public void ConsumeCurrentItem()
    {
        IsHolding = false;
        if (HeldItem != null)
            Destroy(HeldItem.gameObject);
        HeldItem = null;
    }

    // Drops to the ground the item currently held
    public void DropItem()
    {
        if (!IsHolding)
            return;

        IsHolding = false;
        HeldItem.Drop();
        HeldItem = null;
    }

    public void GrabNewItem(Item itemPrefab) => GrabItem(Instantiate(itemPrefab, transform));
    
    public void GrabItem(Item item)
    {
        IsHolding = true;
        HeldItem = item;
    }
    
    private void OnDestroy()
    {
        playerTeam.TeamChanged -= OnTeamChanged;
    }
}
