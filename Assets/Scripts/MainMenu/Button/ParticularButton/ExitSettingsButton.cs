using UnityEngine;
using LitMotion;
using System.Collections;

public class ExitSettingsButton : ActionButton
{
    [SerializeField] private CameraZoomer camZoomer;
    [SerializeField] private SettingsButton settingsButtonScript;
    [SerializeField] private ButtonManager buttonManager;

    private Vector3 initialSettingsButtonPos;

    public void Start()
    {
        initialSettingsButtonPos = settingsButtonScript.transform.position;
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
        float speedButtonWhenClicked = settingsButtonScript.SpeedButtonWhenClicked;
        LMotion.Create(settingsButtonScript.transform.position, initialSettingsButtonPos, speedButtonWhenClicked).WithEase(Ease.OutQuad).Bind(y => settingsButtonScript.gameObject.transform.position = y); ;
        yield return new WaitForSeconds(speedButtonWhenClicked);
        buttonManager.ActivButton();
    }
}
