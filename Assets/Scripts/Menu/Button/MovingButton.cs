using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;

/*Gere Le mouvement des boutons qui se déplace vers la gauche en générales*/
public class MovingButton : ActionButton
{
    [SerializeField] private float finalPositionButtonAfterMove;
    [SerializeField] private float speedButtonWhenClicked;
    [SerializeField] private CameraZoomer camZoomer;

    public float SpeedButtonWhenClicked => speedButtonWhenClicked;
    public override void Action(){}

    protected  virtual void Movement()
    {
        Vector3 targetPosition = this.transform.position + new Vector3(finalPositionButtonAfterMove, 0f, 0f);
        LMotion.Create(this.transform.position, targetPosition, speedButtonWhenClicked).WithEase(Ease.OutQuad).Bind(y => this.transform.position = y);
    }
    
    public override void OnClick()
    {
        Movement();

        Button.interactable = false;
        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomInCoroutineAction());
    }

    private IEnumerator ZoomInCoroutineAction()
    {
        yield return camZoomer.ZoomIn(transform.position);
        Action();
        Button.interactable = true;
    }
}
