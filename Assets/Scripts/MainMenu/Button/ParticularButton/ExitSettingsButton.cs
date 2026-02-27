using UnityEngine;
using LitMotion;
using System.Collections;

public class ExitSettingsButton : ActionButton
{
    [SerializeField] private CameraZoomer camZoomer;
    [SerializeField] private SettingsButton settingsButton;
    [SerializeField] private ButtonManager buttonManager;

    private Vector3 initialSettingsButtonPos;

    public void Start()
    {
        initialSettingsButtonPos = settingsButton.transform.position;
    }
    
    public override void Action()
    {
        StartCoroutine(WaitMovementSettingsButton());
    }

    public override void OnClick()
    {
        Button.interactable = false;
        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomOutCoroutineAction());
    }

    private IEnumerator ZoomOutCoroutineAction()
    {
        yield return camZoomer.ReturnToNormalState();
        Action();
    }

    private IEnumerator WaitMovementSettingsButton()
    {
        float pressedButtonSpeed = settingsButton.PressedButtonSpeed;
        LMotion.Create(settingsButton.transform.position, initialSettingsButtonPos, pressedButtonSpeed).WithEase(Ease.OutQuad).Bind(y => settingsButton.gameObject.transform.position = y); ;
        yield return new WaitForSeconds(pressedButtonSpeed);
        buttonManager.ActivButton();
    }
}
