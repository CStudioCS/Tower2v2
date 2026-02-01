using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerTeam playerTeam;
    
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
    
    [Header("Interactables")]
    public Interactable insideInteractable;
    public bool isHolding = false;
    
    public Item heldItem;

    [Header("Speed")]
    [SerializeField] private float speed = 7;
    
    [Header("Progress")]
    [SerializeField] private RectTransform ProgressBar;
    [SerializeField] private RectTransform ProgressBarFill;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction cutWoodAction;
    private Rigidbody2D rb;
    private float maxProgressBarFill;
    private bool interacting;
    

    private void Awake()
    {
        playerTeam.TeamChanged += OnTeamChanged;
        UpdateColor();
        playerInput = GetComponent<PlayerInput>(); //Using unity's new input system
        moveAction = playerInput.actions.FindAction("Gameplay/Move");
        interactAction = playerInput.actions.FindAction("Gameplay/Interact");
        cutWoodAction = playerInput.actions.FindAction("Gameplay/CutWood");

        rb = GetComponent<Rigidbody2D>();

        maxProgressBarFill = ProgressBarFill.sizeDelta.x;
        ResetProgressBar();
    }

    private void OnTeamChanged() => UpdateColor();
    private void UpdateColor()
    {
        spriteRenderer.color = TeamColors[playerTeam.CurrentTeam];
    }

    void Update()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();

        if (!interacting)
            rb.linearVelocity = move * speed;
        else
            rb.linearVelocity = Vector2.zero;

        if (interactAction.WasPressedThisFrame())
            StartInteracting(true);

        if(cutWoodAction.WasPressedThisFrame())
            StartInteracting(false);
    }

    void StartInteracting(bool interactingWithE)
    {
        //we check whether we're inside an interactable object and if yes if we can interact with it
        if (insideInteractable == null)
            return;

        //If we can interact instantly, we do it, else we need to wait for the interaction time
        if (interactingWithE)
        {
            if (!insideInteractable.CanInteract(this))
                return;

            if (insideInteractable.interactionTime > 0)
                StartCoroutine(InteractTimer(insideInteractable.interactionTime));
            else
                insideInteractable.Interact(this);
        }
        else
        {
            if (!insideInteractable.CanInteractA(this))
                return;

            if (insideInteractable.interactionTimeA > 0)
                StartCoroutine(InteractTimerA(insideInteractable.interactionTimeA));
            else
                insideInteractable.InteractA(this);
        }
        
    }

    private IEnumerator InteractTimer(float time)
    {
        interacting = true;
        ProgressBar.gameObject.SetActive(true);
        float t = 0;
        
        while(t < time)
        {
            //if at any point the player stop holding the interact button -> stop interacting
            if (!interactAction.IsPressed())
            {
                ResetProgressBar();
                interacting = false;
                yield break;
            }

            //fill progress bar
            ProgressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, maxProgressBarFill, t / time), ProgressBarFill.sizeDelta.y);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        //We interacted with the object -> Reset everything and call the interact function

        interacting = false;
        ResetProgressBar();
        insideInteractable.Interact(this);
    }

    
    private IEnumerator InteractTimerA(float time)
    {
        
        interacting = true;
        ProgressBar.gameObject.SetActive(true);
        float t = 0;
        
        while(t < time)
        {
            //if at any point the player stop holding the interact button -> stop interacting
            if (!cutWoodAction.IsPressed())
            {
                ResetProgressBar();
                interacting = false;
                yield break;
            }

            //fill progress bar
            ProgressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, maxProgressBarFill, t / time), ProgressBarFill.sizeDelta.y);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        //We interacted with the object -> Reset everything and call the interact function

        interacting = false;
        ResetProgressBar();
        insideInteractable.InteractA(this);
    }


    private void ResetProgressBar()
    {
        ProgressBar.gameObject.SetActive(false);
        ProgressBarFill.sizeDelta = new Vector2(0, ProgressBarFill.sizeDelta.y);
    }

    public void ConsumeCurrentItem()
    {
        isHolding = false;
        Destroy(heldItem.gameObject);
        heldItem = null;
    }

    //Discards the item currently held
    private void DropHeldItem()
    {
        if (!isHolding)
            return;

        isHolding = false;
        heldItem.Drop();
        heldItem = null;
    }
    
    private void OnDestroy()
    {
        playerTeam.TeamChanged -= OnTeamChanged;
    }
}
