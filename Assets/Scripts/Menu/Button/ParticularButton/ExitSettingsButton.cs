using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
public class ExitSettingsButton : IActionButton
{

    
    [SerializeField] private CameraZoomer camZoomer;
    [SerializeField] private SettingsButton settingsButton;

    private Vector3 initialSettingsButtonPos;

    public void Start()
    {
        initialSettingsButtonPos = settingsButton.gameObject.transform.position;
    }
    
    public override void Action()
    {
        float speedButtonWhenClicked = settingsButton.SpeedButtonWhenClicked;
        
        LMotion.Create(settingsButton.gameObject.transform.position, initialSettingsButtonPos, speedButtonWhenClicked).WithEase(Ease.OutQuad).Bind(y => settingsButton.gameObject.transform.position = y);
    }

    protected override void Movement(){}

    public override void OnClick()
    {
        this.Movement();
        this.button.interactable = false;
        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomOutCoroutineAction());
    }

    private IEnumerator ZoomOutCoroutineAction()
    {
        
        yield return  camZoomer.ReturnToNormalState();
        this.Action();
        
        this.button.interactable = true;
    }

}
