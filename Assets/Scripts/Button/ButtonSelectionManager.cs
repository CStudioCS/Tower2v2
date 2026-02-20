using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonSelectionManager : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private bool pauseSelection = false;

    public int numOfButtons { get; set; }
    private float startTime;
    private bool oneShot = false;

    void Start()
    {
        startTime = Time.time;
    }

    //On utilise le fixedUpdate pour empecher les 2 selections managers de s'executer en même temps (ce qui nous empecher qu'un bouton soit cliquable en annulant sa propre action OnClick())
    private void FixedUpdate()
    {
        if (!pauseSelection)
        {
            //Time.time-startTime >0.5f a pour but d'eviter que la scene se lance et que le bouton soit directement appuyé sans faire exprès
            if (Keyboard.current.enterKey.wasPressedThisFrame && Time.time-startTime >0.5f)
            {
                if (buttons[numOfButtons].TryGetComponent(out NoActionButton actionButton))
                {
                    actionButton.OnClick();
                }
            }
        }
    }

    void Update()
    {
        if (!pauseSelection) { 
            if (Gamepad.current != null)
            {
                GamepadChecker();
            }
            if (Keyboard.current != null)
            {
                KeyboardChecker();
            }

            if (numOfButtons < 0)
            {
                numOfButtons = 0;
            }
            else if (numOfButtons > buttons.Length - 1)
            {
                numOfButtons = buttons.Length - 1;
            }
            buttons[numOfButtons].Select();
        }
    }

    //Il ne s'agit pas d'un setter classique, c'est pourquoi on n'utilise pas la methode facile de declarer des setteurs
    public void PauseSelection(bool boolean)
    {
        pauseSelection = boolean;
        foreach (var button in buttons)
        {
            button.interactable = !pauseSelection;
        }
    }

    private void GamepadChecker()
    {
        if ((Gamepad.current.leftStick.ReadValue().y < -0.5f && !oneShot))
        {
            numOfButtons++;
            oneShot = true;
        }
        else if ((Gamepad.current.leftStick.ReadValue().y > 0.5f && !oneShot))
        {
            numOfButtons--;
            oneShot = true;
        }
        else if (Gamepad.current.leftStick.ReadValue().y > -0.1f && Gamepad.current.leftStick.ReadValue().y < 0.1f)
        {
            oneShot = false;
        }
    }
    private void KeyboardChecker()
    {
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            numOfButtons++;
            oneShot = true;
        }
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            numOfButtons--;
            oneShot = true;
        }
    }
}
