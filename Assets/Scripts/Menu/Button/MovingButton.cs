using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;


/*Gere Le mouvement des boutons qui se déplace vers la gauche en générales*/
public class MovingButton : IActionButton
{
    [SerializeField] private float finalPositionButtonAfterMove;
    [SerializeField] private float speedButtonWhenClicked;
    [SerializeField] private CameraZoomer camZoomer;

    protected float speedButtonWhenCLicked => speedButtonWhenClicked;
    public float SpeedButtonWhenClicked => speedButtonWhenClicked;
    public override void Action(){}

    protected  override void Movement()
    {
        Vector3 targetPosition = this.transform.position + new Vector3(finalPositionButtonAfterMove, 0f, 0f);
        LMotion.Create(this.transform.position, targetPosition, speedButtonWhenClicked).WithEase(Ease.OutQuad).Bind(y => this.transform.position = y);

    }
    public override void OnClick()
    {
        this.Movement();

        this.button.interactable = false;
        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomInCoroutineAction());
    }

    private IEnumerator ZoomInCoroutineAction()
    {
        yield return camZoomer.ZoomIn(this.gameObject.transform.position);
        this.Action();
        this.button.interactable = true;
    }
}
