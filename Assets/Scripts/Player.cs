using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Interactable insideInteractable;
    public bool isHolding = false;
    
    public Item heldItem;

    [SerializeField] private float speed = 7;
    [SerializeField] private RectTransform ProgressBar;
    [SerializeField] private RectTransform ProgressBarFill;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction interactAction;
    
    private InputAction interactActionA;
    private InputAction discardAction;
    private Rigidbody2D rb;
    private float maxProgressBarFill;
    private bool interacting;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>(); //Using unity's new input system
        moveAction = playerInput.actions.FindAction("Player/Move");
        interactAction = playerInput.actions.FindAction("Player/Interact");
        discardAction = playerInput.actions.FindAction("Player/Discard");
        interactActionA = playerInput.actions.FindAction("Player/CutWood");

        rb = GetComponent<Rigidbody2D>();

        maxProgressBarFill = ProgressBarFill.sizeDelta.x;
        ResetProgressBar();
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

        if(interactActionA.WasPressedThisFrame())
            StartInteracting(false);

        //Pressing A to drop an item
        if (discardAction.WasPressedThisFrame())
            DropHeldItem();
    }

    void StartInteracting(bool interactingWithE)
    {
        //we check whether we're inside an interactable object and if yes if we can interact with it
        if (insideInteractable == null)
            return;
        if (insideInteractable.isAlreadyInteractedWith)//I do not set isAlreadyInteractedWith to true in an else statement because it only applies to interactables with interaction time
            return;

        //If we can interact instantly, we do it, else we need to wait for the interaction time
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
        insideInteractable.isAlreadyInteractedWith = false;
    }

    
    private IEnumerator InteractTimerA(float time)
    {
        
        interacting = true;
        ProgressBar.gameObject.SetActive(true);
        float t = 0;
        
        while(t < time)
        {
            //if at any point the player stop holding the interact button -> stop interacting
            if (!interactActionA.IsPressed())
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
        insideInteractable.isAlreadyInteractedWith = false;
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
}
