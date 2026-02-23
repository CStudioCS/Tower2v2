using UnityEngine;

public class NoConfirmationButton : NoActionButton
{
    [SerializeField] private ButtonSelectionManager buttonSelection;
    [SerializeField] private GameObject confirmationGameObject;

    public override void OnClick()
    {
        confirmationGameObject.SetActive(false);
        buttonSelection.PauseSelection(false);
    }
}
