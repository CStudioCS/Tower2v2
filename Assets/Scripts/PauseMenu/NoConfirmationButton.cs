using UnityEngine;

public class NoConfirmationButton : NoActionButton
{
    [SerializeField] private ConfirmationPopupContainer confirmationPopupContainer;

    public override void OnClick()
    {
        confirmationPopupContainer.ClosePopUp();
    }
}
