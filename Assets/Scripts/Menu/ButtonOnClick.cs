using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
public class ButtonOnClick : MonoBehaviour
{
    [SerializeField] CameraZoomer camZoomer;

    public void ZoomInOnClick(GameObject buttonClicked)
    {
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Movement();

        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomInCoroutineAction(buttonClicked));
    }

    public void ZoomOutOnClick(GameObject buttonClicked)
    {
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Movement();

        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomOutCoroutineAction(buttonClicked));

    }

    private IEnumerator ZoomOutCoroutineAction(GameObject buttonClicked)
    {
        
        yield return  camZoomer.ReturnToNormalState();
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Action();
    }

    private IEnumerator ZoomInCoroutineAction(GameObject buttonClicked)
    {
        
        yield return camZoomer.ZoomIn(buttonClicked.transform.position);
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Action();
    }

}
