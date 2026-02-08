using UnityEngine;
using LitMotion;
using LitMotion.Extensions;


/*Gere Le mouvement des boutons qui se déplace vers la gauche en générales*/
public class ButtonActionWithMovement : ButtonAction
{
    [SerializeField] private float finalPositionButtonAfterMove;
    [SerializeField] private float speedButtonWhenClicked;

    public float SpeedButtonWhenClicked => speedButtonWhenClicked;
    public override void Action(){}

    public override void Movement()
    {
        Vector3 targetPosition = this.transform.position + new Vector3(finalPositionButtonAfterMove, 0f, 0f);
        LMotion.Create(this.transform.position, targetPosition, speedButtonWhenClicked).WithEase(Ease.OutQuad).Bind(y => this.transform.position = y);

    }

    public float getSpeedButtonWhenClicked()
    {
        return speedButtonWhenClicked;
    }
}
