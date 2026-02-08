using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using LitMotion;
using LitMotion.Extensions;

public class ButtonClick : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private float zoomSize;
    private float initialCamSize;
    private Vector3 initialCamPosition;

    void Start()
    {
        initialCamSize = cam.orthographicSize;
        initialCamPosition = cam.transform.position;
    }

    public void ZoomInOnCLick(GameObject buttonClicked)
    {
        float posYFinal = buttonClicked.transform.position.y;

        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Movement();

        StartCoroutine(Zoom.TransitionZoomInCoroutine(cam, initialCamSize, initialCamPosition, posYFinal, zoomSize, buttonAction.Action));
    
    }
    

    public void ZoomOutOnCLick(GameObject buttonClicked)
    {
        ButtonAction buttonAction = buttonClicked.GetComponent<ButtonAction>();
        buttonAction.Movement();

        StartCoroutine(Zoom.TransitionZoomInCoroutine(cam, zoomSize, cam.transform.position, initialCamPosition.y, initialCamSize, buttonAction.Action));
    }
}
