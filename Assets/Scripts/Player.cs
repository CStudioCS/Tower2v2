using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Interactable insideInteractable;
    public HeldItem heldItem;
    public GameObject heldItemGameobject;

    [SerializeField] private float speed = 7;
    [SerializeField] private RectTransform ProgressBar;
    [SerializeField] private RectTransform ProgressBarFill;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction interactAction;
    private InputAction discardAction;
    private Rigidbody2D rb;
    private float maxFill;
    private bool interacting;
    public enum HeldItem { Nothing, Wood, Brick, Cement, Dirt }

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Player/Move");
        interactAction = playerInput.actions.FindAction("Player/Interact");
        discardAction = playerInput.actions.FindAction("Player/Discard");
        rb = GetComponent<Rigidbody2D>();

        maxFill = ProgressBarFill.sizeDelta.x;
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
        if (discardAction.WasPressedThisFrame())
            DiscardItem();
    }

    void StartInteracting()
    {
        if (insideInteractable == null || !insideInteractable.CanInteract(this))
            return;

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
            if (!interactAction.IsPressed())
            {
                ResetProgressBar();
                interacting = false;
                yield break;
            }

            ProgressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, maxFill, t / time), ProgressBarFill.sizeDelta.y);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        interacting = false;
        ResetProgressBar();
        insideInteractable.Interact(this);
    }

    private void ResetProgressBar()
    {
        ProgressBar.gameObject.SetActive(false);
        ProgressBarFill.sizeDelta = new Vector2(0, ProgressBarFill.sizeDelta.y);
    }

    private void DiscardItem()
    {
        heldItem = HeldItem.Nothing;
        if (heldItemGameobject != null)
            Destroy(heldItemGameobject);
        heldItemGameobject = null;
    }
}
