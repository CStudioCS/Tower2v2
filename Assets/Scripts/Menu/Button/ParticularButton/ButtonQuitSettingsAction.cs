using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
public class ButtonQuitSettingsAction : ButtonAction
{

    [SerializeField] private GameObject settingsButton;
    [SerializeField] private ButtonQuitSettingsAction settingButtonAction;

    private Vector3 initialSettingsButtonPos;

    public void Start()
    {
        initialSettingsButtonPos = settingsButton.transform.position;
    }
    
    public override void Action()
    {
        float speedButtonWhenClicked = settingsButton.GetComponent<ButtonActionWithMovement>().SpeedButtonWhenClicked;
        
        LMotion.Create(settingsButton.transform.position, initialSettingsButtonPos, speedButtonWhenClicked).WithEase(Ease.OutQuad).Bind(y => settingsButton.transform.position = y);
    }

    public override void Movement(){}

}
