using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
public class ButtonQuitSettingsAction : ButtonAction
{

    [SerializeField] private GameObject settingsButton;

    private Vector3 settingsButtonInitialPos;

    public void Start()
    {
        settingsButtonInitialPos = settingsButton.transform.position;
    }
    
    public override void Action()
    {
        Vector3 targetPosition = settingsButtonInitialPos;
        float speedButtonWhenClicked = settingsButton.GetComponent<ButtonActionWithMovement>().SpeedButtonWhenClicked;
        
        LMotion.Create(settingsButton.transform.position, targetPosition, speedButtonWhenClicked).WithEase(Ease.OutQuad).Bind(y => settingsButton.transform.position = y);
    }

    public override void Movement(){}

}
