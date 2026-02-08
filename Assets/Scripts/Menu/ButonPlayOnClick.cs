using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ButtonClicked : MonoBehaviour
{
    [SerializeField] private GameObject buttonPlay;
    
    [SerializeField] private GameObject buttonSettings;
    
    [SerializeField] private GameObject buttonQuit;

    [SerializeField] private Camera cam;

    [SerializeField] private float finalPositionButtonAfterMove = 15f;

    
    [SerializeField] private float speedButtonWhenClicked = 0.8f;

    private float initialCamSize;
    private Vector3 initialPositionButonSettings;
    private Vector3 initialCamPosition;
    private float posYFinal;

    void Start()
    {
        initialCamSize = cam.orthographicSize;
        initialCamPosition = cam.transform.position;
        initialPositionButonSettings = buttonSettings.gameObject.transform.position;
    }

    public void PlayButtonClicked()
    {
        //Utilisation de DOTween 
        buttonPlay.transform.DOMoveX(finalPositionButtonAfterMove, speedButtonWhenClicked).SetEase(Ease.OutQuad);
        posYFinal = buttonPlay.transform.position.y;
        StartCoroutine(Zoom.TransitionZoomInCoroutine(cam, initialCamSize, initialCamPosition, posYFinal,SceneManager.LoadScene));
    }

    public void SettingsButtonClicked()
    {
        //Utilisation de DOTween 
        buttonSettings.transform.DOMoveX(finalPositionButtonAfterMove, speedButtonWhenClicked).SetEase(Ease.OutQuad);
        posYFinal = buttonSettings.transform.position.y;
        StartCoroutine(Zoom.TransitionZoomInCoroutine(cam, initialCamSize, initialCamPosition, posYFinal));
    }

    public void QuitButtonClicked()
    {
        //Utilisation de DOTween 
        buttonQuit.transform.DOMoveX(finalPositionButtonAfterMove, speedButtonWhenClicked).SetEase(Ease.OutQuad);
        posYFinal = buttonQuit.transform.position.y;
        StartCoroutine(Zoom.TransitionZoomInCoroutine(cam, initialCamSize, initialCamPosition, posYFinal));
        
        Application.Quit();
    }

    public void QuitSettingsButtonClicked()
    {
        //Utilisation de DOTween 
        buttonSettings.transform.DOMoveX(initialPositionButonSettings.x, speedButtonWhenClicked).SetEase(Ease.InQuad);

        StartCoroutine(Zoom.TransitionZoomOutCoroutine(cam, initialCamSize, initialCamPosition, initialPositionButonSettings.y));
    }
}
