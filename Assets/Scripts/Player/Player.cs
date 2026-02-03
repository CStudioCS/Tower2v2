using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerTeam playerTeam;
    public PlayerTeam PlayerTeam => playerTeam;
    [SerializeField] private PlayerControlBadge playerControlBadge;
    public PlayerControlBadge PlayerControlBadge => playerControlBadge;
    
    [Header("Colors")]
    [SerializeField] private Color leftTeamColor;
    [SerializeField] private Color rightTeamColor;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
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
    
    [Header("Progress")]
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progressBarFill;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private InputAction cutWoodAction;
    
    private float maxProgressBarFill;
    public bool Interacting { get; private set; }

    private void Awake()
    {
        playerTeam.TeamChanged += OnTeamChanged;
        UpdateColor();
        playerInput = GetComponent<PlayerInput>(); // Using Unity's new input system
        interactAction = playerInput.actions.FindAction("Gameplay/Interact");
        cutWoodAction = playerInput.actions.FindAction("Gameplay/CutWood");

        maxProgressBarFill = progressBarFill.sizeDelta.x;
        ResetProgressBar();
    }

    private void OnTeamChanged() => UpdateColor();
    private void UpdateColor()
    {
        spriteRenderer.color = TeamColors[playerTeam.CurrentTeam];
    }

    void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            switch (LevelManager.Instance.GameState)
            {
                case LevelManager.State.Game:
                    StartInteracting(true);
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
                    StartInteracting(false);
                    break;
            }
        }
    }

    void StartInteracting(bool interactingWithE)
    {
        // We check whether we're inside an interactable object and if yes if we can interact with it
        if (insideInteractable == null)
            return;
        if (insideInteractable.isAlreadyInteractedWith) // I do not set isAlreadyInteractedWith to true in an else statement because it only applies to interactables with interaction time
            return;

        // If we can interact instantly, we do it, else we need to wait for the interaction time
        if (interactingWithE)
        {
            if (!insideInteractable.CanInteract(this))
                return;

            if (insideInteractable.interactionTime > 0)
            {
                insideInteractable.isAlreadyInteractedWith = true;
                StartCoroutine(InteractTimer(insideInteractable.interactionTime));
            }
            else
                insideInteractable.Interact(this);
        }
        else
        {
            if (!insideInteractable.CanInteractA(this))
                return;

            if (insideInteractable.interactionTimeA > 0)
            {
                insideInteractable.isAlreadyInteractedWith = true;
                StartCoroutine(InteractTimerA(insideInteractable.interactionTimeA));
            }
            else
                insideInteractable.InteractA(this);
        }
        
    }

    private IEnumerator InteractTimer(float time)
    {
        Interacting = true;
        progressBar.gameObject.SetActive(true);
        float t = 0;
        
        while(t < time)
        {
            // If at any point the player stop holding the interact button -> stop interacting
            if (!interactAction.IsPressed())
            {
                ResetProgressBar();
                Interacting = false;
                insideInteractable.isAlreadyInteractedWith = false;
                yield break;
            }

            // Fill progress bar
            progressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, maxProgressBarFill, t / time), progressBarFill.sizeDelta.y);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        // We interacted with the object -> Reset everything and call the interact function

        Interacting = false;
        ResetProgressBar();
        insideInteractable.Interact(this);
        insideInteractable.isAlreadyInteractedWith = false;
    }
    
    private IEnumerator InteractTimerA(float time)
    {
        Interacting = true;
        progressBar.gameObject.SetActive(true);
        float t = 0;
        
        while (t < time)
        {
            // If at any point the player stop holding the interact button -> stop interacting
            if (!cutWoodAction.IsPressed())
            {
                ResetProgressBar();
                Interacting = false;
                yield break;
            }

            // Fill progress bar
            progressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, maxProgressBarFill, t / time), progressBarFill.sizeDelta.y);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        // We interacted with the object -> Reset everything and call the interact function

        Interacting = false;
        ResetProgressBar();
        insideInteractable.InteractA(this);
        insideInteractable.isAlreadyInteractedWith = false;
    }

    private void ResetProgressBar()
    {
        progressBar.gameObject.SetActive(false);
        progressBarFill.sizeDelta = new Vector2(0, progressBarFill.sizeDelta.y);
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
    
    public void GrabNewItem(GameObject itemPrefab) => GrabItem(Instantiate(itemPrefab, transform).GetComponent<Item>());
    
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
