using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;
public class SelectionManagement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playText;
    [SerializeField] private TextMeshProUGUI settingsText;
    [SerializeField] private TextMeshProUGUI quitText;
    [SerializeField] private Camera cam;

    private bool isZoomingIn = false;
    private Keyboard keyboard;
    private int nbSelected = 0;
    private float inputDelay = 0.2f;
    private float initialCamSize;
    private Vector3 initialCamPosition;
    private float time = 0f;
    private bool isAButtonCLicked = false;


    void Start()
    {
        // Store initial camera parameters
        initialCamSize = cam.orthographicSize;
        initialCamPosition = cam.transform.position;

        // Get the keyboard instance
        keyboard = Keyboard.current;
    }

    void Update()
    {
        // Only allow selection if not zooming in and if no button is currently clicked
        if(!isAButtonCLicked && !isZoomingIn){
            Selection();
            // Check for selection confirmation
            if(keyboard.enterKey.wasPressedThisFrame)
                Transition();
        }
        // Update the display based on current selection
        AffichageSelection();

        if(isZoomingIn)
            ZoomIn();
        
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

    private void Transition()
    {
        // Deactivate all menu texts
        playText.gameObject.SetActive(false); 
        settingsText.gameObject.SetActive(false);
        quitText.gameObject.SetActive(false);

        //select the correct parent to zoom in on
        Transform parent = null;

        switch (nbSelected)
                {
                    case 0:
                        parent = playText.gameObject.transform.parent;
                        break;
                    case 1:
                        parent = settingsText.gameObject.transform.parent;
                        break;
                    case 2:
                        parent = quitText.gameObject.transform.parent;
                        break;
                }

        parent.gameObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.right * 35f;

        //Start the zoom in process
        isZoomingIn = true;

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
                pos = playText.transform.parent.gameObject.transform.position.y;
                break;
            case 1:
                pos = settingsText.transform.parent.gameObject.transform.position.y;
                break;
            case 2:
                pos = quitText.transform.parent.gameObject.transform.position.y;
                break;
        }
        
        //move the camera to the correct position using easing
        cam.transform.position = Vector3.Lerp(initialCamPosition, new Vector3(4.5f, pos, -10f), Easing.QuadOut(time));
        
        //if the zoom in is complete, load the appropriate scene or quit
        if(time >= 0.99f && nbSelected == 0){
            SceneManager.LoadScene(0);
        }
        else if(time >= 0.99f && nbSelected == 2){
            Application.Quit();
        }
        
    }

    private IEnumerator TransitionCoroutine(){
        while (time < 1){
            time += Time.deltaTime;
            yield return null;
        }
    }
}


