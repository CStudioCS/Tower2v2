using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
public class SelectionManagement : MonoBehaviour
{
    [SerializeField] private GameObject playRectangle;
    [SerializeField] private TextMeshProUGUI playText;
    
    [SerializeField] private GameObject settingsRectangle;
    [SerializeField] private TextMeshProUGUI settingsText;
    
    [SerializeField] private TextMeshProUGUI settingsQuitText;
    [SerializeField] private GameObject settingsQuitRectangle;

    [SerializeField] private TextMeshProUGUI quitText;
    [SerializeField] private GameObject quitRectangle;

    [SerializeField] private Camera cam;


    private GameObject selectedGameObject = null;
    private Keyboard keyboard;

    private bool isZoomingIn = false;
    private bool isZoomingOut = false;

    private int nbSelected = 0;
    private float inputDelay = 0.2f;
    private float time = 0f;

    private float initialCamSize;
    private Vector3 initialPositionButonSettings;
    private Vector3 initialCamPosition;

    private bool isAButtonCLicked = false;
    private bool updateSettings = false;


    void Start()
    {
        // Store initial camera parameters
        initialCamSize = cam.orthographicSize;
        initialCamPosition = cam.transform.position;

        initialPositionButonSettings = settingsRectangle.transform.position;

        // Get the keyboard instance
        keyboard = Keyboard.current;
    }

    void Update()
    {
        // Only allow selection if not zooming in and if no button is currently clicked
        
        // Update the display based on current selection
        AffichageSelection();

        if(isZoomingIn)
            ZoomIn();
        else if(isZoomingOut)
        {
            ZoomOut();
        }
        else if (updateSettings)
        {
            UpdateSettings();
        }
        else if(!isAButtonCLicked && !isZoomingIn && !updateSettings && !isZoomingOut){
            Selection();
            // Check for selection confirmation
            if(keyboard.enterKey.wasPressedThisFrame)
            
                Transition();
        }
        
    }
    private void AffichageSelection()
    {
        // Reset all font sizes
        playText.fontSize = 96;
        settingsText.fontSize = 96;
        quitText.fontSize = 96;

        // Highlight the selected option
        switch (nbSelected)
        {
            case 0:
                playText.fontSize = 150;
                break;
            case 1:
                settingsText.fontSize = 150;
                break;
            case 2:
                quitText.fontSize = 150;
                break;
        }
    }
    private void Selection()
    {   
        // Check for keyboard input
        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            nbSelected--;
            isAButtonCLicked = true;
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            nbSelected++;
            isAButtonCLicked = true;
        }

        // Check for gamepad input
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.leftStick.ReadValue().y > 0.1f)
            {
                nbSelected--;
                isAButtonCLicked = true;
            }
            else if (gamepad.leftStick.ReadValue().y < -0.1f)
            {
                nbSelected++;
                isAButtonCLicked = true;
            }
            
        }

        // Clamp the selection between 0 and 2
        if (nbSelected < 0)
        {
            nbSelected = 0;
        }
        else if (nbSelected > 2)
        {
            nbSelected = 2;
        }

        // Start cooldown to prevent multiple inputs
        if(isAButtonCLicked)
        {
            StartCoroutine(InputCooldown());
        }

        
    }   
    private IEnumerator InputCooldown()
    {        
        yield return new WaitForSeconds(inputDelay);
        isAButtonCLicked = false;
    }

    private IEnumerator InputCooldown2()
    {        
        yield return new WaitForSeconds(1f);
        isAButtonCLicked = false;
    }
    private void Transition()
    {
        // Deactivate all menu texts
        playText.gameObject.SetActive(false); 
        settingsText.gameObject.SetActive(false);
        quitText.gameObject.SetActive(false);

        //select the correct rectangle to zoom in on

        switch (nbSelected)
                {
                    case 0:
                        selectedGameObject = playRectangle ;
                        break;
                    case 1:
                        selectedGameObject = settingsRectangle;
                        break;
                    case 2:
                        selectedGameObject = quitRectangle;
                        break;
                }
        
        //Ajout d'une force pour faire l'effet que le rectangle parte
        selectedGameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * 35f;
       
        //Start the zoom in process
        isZoomingIn = true;

        time = 0f;
        //Coroutine qui gère le temps pour l'easing
        StartCoroutine(TransitionCoroutine());
        
    }

    private void ZoomIn()
    {
        //zoom in the camera using easing
        cam.orthographicSize = Mathf.Lerp(initialCamSize, 0f, Easing.QuadOut(time));
        
        //select the correct y position to zoom in on

        float pos = 0f;
        switch(nbSelected)
        {
            case 0:
                pos = playRectangle.transform.position.y;
                break;
            case 1:
                pos = settingsRectangle.transform.position.y;
                break;
            case 2:
                pos = quitRectangle.transform.position.y;
                break;
        }
        
        //move the camera to the correct position using easing
        cam.transform.position = Vector3.Lerp(initialCamPosition, new Vector3(4.5f, pos, -10f), Easing.QuadOut(time));
        
        //if the zoom in is complete, load the appropriate scene or quit
        if(time > 1f && nbSelected == 0){
            selectedGameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            SceneManager.LoadScene(0);
        }
        else if(time > 1f && nbSelected == 2){
            selectedGameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            Application.Quit();
        }
        else if(time >  1f && nbSelected == 1){

            selectedGameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

            settingsQuitRectangle.gameObject.SetActive(true);
            settingsQuitText.gameObject.SetActive(true);
        
            isZoomingIn = false;
            updateSettings = true;
            
            StopCoroutine(TransitionCoroutine());
        }
        
    }

    private IEnumerator TransitionCoroutine(){
        while (time < 1){
            time += Time.deltaTime;
            yield return null;
        }
    }

    void UpdateSettings()
    {
        if(keyboard.enterKey.wasPressedThisFrame)
        {
            isZoomingOut = true;
            updateSettings = false;
            time = 0f;
            
            selectedGameObject.GetComponent<Rigidbody2D>().linearVelocity = -Vector2.right * 35f;
            StartCoroutine(TransitionCoroutine());
        }
         foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.buttonSouth.wasPressedThisFrame)
            {
                isZoomingOut = true;
                updateSettings = false;
                time = 0f;
                selectedGameObject.GetComponent<Rigidbody2D>().linearVelocity = -Vector2.right * 35f;
            
            }
        }
    }
    void TransitionBackToMenu()
    {
        // Reset camera parameters
        cam.orthographicSize = initialCamSize;
        cam.transform.position = initialCamPosition;

        // Reactivate menu texts
        playText.gameObject.SetActive(true); 
        settingsText.gameObject.SetActive(true);
        quitText.gameObject.SetActive(true);

        // Deactivate settings menu

        selectedGameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
           
        selectedGameObject.transform.position = initialPositionButonSettings;
        // Reset selection and states
        nbSelected = 1;

        isZoomingIn = false;
        isZoomingOut = false;
        updateSettings = false;

        isAButtonCLicked = false;
        
        //Rajout d'une pose car il y a un bug qui fait que le joueur peut appuyer sur la touche pour zoomer alors que le rectangle n'est pas encore revenu à sa position initiale
        StopCoroutine(TransitionCoroutine());
    }

    void ZoomOut()
    {
        settingsQuitRectangle.gameObject.SetActive(false);
        settingsQuitText.gameObject.SetActive(false);
        //zoom out the camera using easing
        cam.orthographicSize = Mathf.Lerp(0f, initialCamSize, Easing.RootOut(time));

        //move the camera to the correct position using easing
        cam.transform.position = Vector3.Lerp(new Vector3(4.5f, initialPositionButonSettings.y, -10f), initialCamPosition, Easing.QuadOut(time));
        
        //if the zoom out is complete, reset everything
        if(time > 1f){
            TransitionBackToMenu();
        }
    }

}


