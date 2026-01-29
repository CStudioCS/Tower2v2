using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Interactable insideInteractable;
    public bool isHolding = false;
    public Resources.Type heldItem;
    public GameObject heldItemGameobject;

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
            StartInteracting();

        if(interactActionA.WasPressedThisFrame())
        {
            StartInteractingA();
        }

        //Pressing A to discard an item
        //Would be nice to remove this and make a trashcan interactable instead.
        if (discardAction.WasPressedThisFrame())
            DiscardHeldItem();
    }

    void StartInteracting()
    {
        //we check whether we're inside an interactable object and if yes if we can interact with it
        if (insideInteractable == null || !insideInteractable.CanInteract(this))
            return;

        //If we can interact instantly, we do it, else we need to wait for the interaction time
        if (insideInteractable.interactionTime > 0)
            StartCoroutine(InteractTimer(insideInteractable.interactionTime));
        else
            insideInteractable.Interact(this);
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

    void StartInteractingA()
    {
        
        //we check whether we're inside an interactable object and if yes if we can interact with it
        if (insideInteractable == null || !insideInteractable.CanInteract(this))
            return;

        //If we can interact instantly, we do it, else we need to wait for the interaction time
        if (insideInteractable.interactionTimeA > 0){
            StartCoroutine(InteractTimerA(insideInteractable.interactionTimeA));
        }
        else
            insideInteractable.InteractA(this);
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
    }


    private void ResetProgressBar()
    {
        ProgressBar.gameObject.SetActive(false);
        ProgressBarFill.sizeDelta = new Vector2(0, ProgressBarFill.sizeDelta.y);
    }

    //Discards the item currently held
    private void DiscardHeldItem()
    {
        isHolding = false;
        if (heldItemGameobject != null)
            Destroy(heldItemGameobject);
        heldItemGameobject = null;
    }

    public void Drop()
    {
        throw new System.NotImplementedException();
    }
}
