using UnityEngine;
using LitMotion;
using LitMotion.Extensions;
using System.Collections;
public class ButtonClick : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private float zoomSize;
    [SerializeField] private float zoomDuration = 1f;

    private float initialCamSize;
    private Vector3 initialCamPosition;

    void Start()
    {
        initialCamSize = cam.orthographicSize;
        initialCamPosition = cam.transform.position;
    }

    public void ZoomInOnCLick(GameObject buttonClicked)
    {
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Movement();

        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomInCoroutineAction(buttonClicked));
    }

    public void ZoomOutOnCLick(GameObject buttonClicked)
    {
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Movement();

        //La fonction doit retourner void pour le OnClick donc elle passe par une autre fonction qui lance l'action après le zoom
        StartCoroutine(ZoomOutCoroutineAction(buttonClicked));

    }

    private IEnumerator ZoomOutCoroutineAction(GameObject buttonClicked)
    {
        yield return Zoom.TransitionZoomInCoroutine(cam, zoomDuration,zoomSize, cam.transform.position, initialCamPosition, initialCamSize);
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Action();
    }

    private IEnumerator ZoomInCoroutineAction(GameObject buttonClicked)
    {
        yield return Zoom.TransitionZoomInCoroutineAction(cam, zoomDuration,initialCamSize, initialCamPosition, buttonClicked.transform.position, zoomSize);
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Action();
    }

}
