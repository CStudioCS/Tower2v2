using UnityEngine;

public class YesConfirmationButton : NoActionButton
{
    [SerializeField] private ActionButton actionButton;
    public override void OnClick()
    {
        actionButton.Action();
    }
}
