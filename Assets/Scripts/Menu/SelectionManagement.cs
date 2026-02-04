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
        initialCamSize = cam.orthographicSize;
        initialCamPosition = cam.transform.position;
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if(!isAButtonCLicked){
            Selection();
            if(keyboard.enterKey.wasPressedThisFrame)
            {
                Transition();
            }
        }
        AffichageSelection();
        if(isZoomingIn)
        {   

            ZoomIn();
        }
    }
    private void AffichageSelection()
    {
        playText.fontSize = 96;
        settingsText.fontSize = 96;
        quitText.fontSize = 96;
        switch (nbSelected)
                {
                    case 0:
                        playText.fontSize = 150;
                        // Load the game scene or start the game
                        break;
                    case 1:
                        settingsText.fontSize = 150;
                        // Open the settings menu
                        break;
                    case 2:
                        quitText.fontSize = 150;
                        // Quit the application
                        break;
                }
    }
    private void Selection()
    {   

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

        if (nbSelected < 0)
        {
            nbSelected = 0;
        }
        else if (nbSelected > 2)
        {
            nbSelected = 2;
        }

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
        playText.gameObject.SetActive(false);
        settingsText.gameObject.SetActive(false);
        quitText.gameObject.SetActive(false);

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
        isZoomingIn = true;
        StartCoroutine(TransitionCoroutine());
        
    }

    private void ZoomIn()
    {

        cam.orthographicSize = Mathf.Lerp(initialCamSize, 0f, Easing.QuadOut(time));
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
            case default:
                break;
        }
        

        cam.transform.position = Vector3.Lerp(initialCamPosition, new Vector3(4.5f, pos, -10f), Easing.QuadOut(time));
        
        if(time >= 0.99f && nbSelected == 0){
            SceneManager.LoadScene(0);
        }
        
    }

    private IEnumerator TransitionCoroutine(){
        while (time < 1){
            time += Time.deltaTime;
            yield return null;
        }
    }
}


