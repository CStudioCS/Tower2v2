using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;

/*Gere Le mouvement des boutons qui se déplace vers la gauche en générales*/
public class MovingButton : ActionButton
{
    [SerializeField] private float xOffset;
    [SerializeField] private float pressedButtonSpeed;
    [SerializeField] private CameraZoomer camZoomer;
    [SerializeField] private ButtonManager buttonManager;

    public float PressedButtonSpeed => pressedButtonSpeed;
    public override void Action(){}

    protected virtual void Movement()
    {
        Vector3 targetPosition = transform.position + new Vector3(xOffset, 0f, 0f);
        LMotion.Create(transform.position, targetPosition, pressedButtonSpeed).WithEase(Ease.OutQuad).Bind(y => transform.position = y);
    }
    
    public override void OnClick()
    {
        Movement();
        buttonManager.DesactivButton();
        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomInCoroutineAction());
    }

    private IEnumerator ZoomInCoroutineAction()
    {
        yield return camZoomer.ZoomIn(transform.position);
        Action();
    }
}
