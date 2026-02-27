using UnityEngine;

public class NoConfirmationButton : NoActionButton
{
    [SerializeField] private ConfirmationPopUp confirmationPopUp;

    public override void OnClick()
    {
        confirmationPopUp.ClosePopUp();
    }
}
