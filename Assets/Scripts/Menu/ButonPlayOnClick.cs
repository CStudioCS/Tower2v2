using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ButtonClicked : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject buttonPlay;
    
    [SerializeField] private GameObject buttonSettings;
    
    [SerializeField] private GameObject buttonQuit;

    [SerializeField] private Camera cam;



    private float initialCamSize;
    private Vector3 initialPositionButonSettings;
    private Vector3 initialCamPosition;
    private float posyFinal = 0f;

    void Start()
    {
        // Store initial camera parameters
        initialCamSize = cam.orthographicSize;
        initialCamPosition = cam.transform.position;

        initialPositionButonSettings = buttonSettings.gameObject.transform.position;
    }

    public void PlayButtonClicked()
    {
        // Load the game scene
        buttonPlay.transform.DOMoveX(15f, 0.8f).SetEase(Ease.OutQuad);
        posyFinal = buttonPlay.transform.position.y;
        StartCoroutine(Zoom.TransitionZoomInCoroutine(cam, initialCamSize, initialCamPosition, posyFinal,SceneManager.LoadScene));
        
    }

    public void SettingsButtonClicked()
    {
        // Load the settings scene
        
        buttonSettings.transform.DOMoveX(15f, 0.8f).SetEase(Ease.OutQuad);
        posyFinal = buttonSettings.transform.position.y;
        StartCoroutine(Zoom.TransitionZoomInCoroutine(cam, initialCamSize, initialCamPosition, posyFinal));
    }

    public void QuitButtonClicked()
    {
        
        buttonQuit.transform.DOMoveX(15f, 0.8f).SetEase(Ease.OutQuad);
        posyFinal = buttonQuit.transform.position.y;
        StartCoroutine(Zoom.TransitionZoomInCoroutine(cam, initialCamSize, initialCamPosition, posyFinal));
        // Quit the application
        Application.Quit();
    }

    public void QuitSettingsButtonClicked()
    {
        // Load the main menu scene
        buttonSettings.transform.DOMoveX(0f, 0.8f).SetEase(Ease.InQuad);
        posyFinal = initialPositionButonSettings.y;
        StartCoroutine(Zoom.TransitionZoomOutCoroutine(cam, initialCamSize, initialCamPosition, posyFinal));
    }
}
