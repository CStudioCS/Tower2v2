using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Furnace : Interactable
{
    [SerializeField] private float cookTime;
    [SerializeField] private RectTransform ProgressBarFill;
    [SerializeField] private GameObject brickPiecePrefab;

    private State state;
    private float maxFill;
    private enum State { Empty, Cooking, Cooked }

    private void Awake()
    {
        maxFill = ProgressBarFill.sizeDelta.x;
        ResetProgressBar();
    }

    public override bool CanInteract(Player player)
    {
        switch (state)
        {
            case State.Empty:
                return player.heldItem == Player.HeldItem.Dirt;
            case State.Cooking:
                return false;
            case State.Cooked:
                return player.heldItem == Player.HeldItem.Nothing;
            default:
                throw new UnityException("Furnace state not handled in CanInteract");
        }
    }

    public override void Interact(Player player)
    {
        if(state == State.Empty)
        {
            StartCoroutine(Cook());

            player.heldItem = Player.HeldItem.Nothing;
            Destroy(player.heldItemGameobject);
            player.heldItemGameobject = null;
        }
        else if(state == State.Cooked)
        {
            player.heldItem = Player.HeldItem.Brick;
            player.heldItemGameobject = Instantiate(brickPiecePrefab, player.transform);
            state = State.Empty;
            ResetProgressBar();
        }
    }

    private IEnumerator Cook()
    {
        state = State.Cooking;

        ProgressBarFill.parent.gameObject.SetActive(true);
        float t = 0;

        while (t < cookTime)
        {
            ProgressBarFill.sizeDelta = new Vector2(Mathf.Lerp(0, maxFill, t / cookTime), ProgressBarFill.sizeDelta.y);

            t += Time.deltaTime * Time.timeScale;
            yield return null;
        }

        ProgressBarFill.sizeDelta = new Vector2(maxFill, ProgressBarFill.sizeDelta.y);
        state = State.Cooked;
    }

    private void ResetProgressBar()
    {
        ProgressBarFill.parent.gameObject.SetActive(false);
        ProgressBarFill.sizeDelta = new Vector2(0, ProgressBarFill.sizeDelta.y);
    }
}
