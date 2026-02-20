using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseButton : NoActionButton
{
    [SerializeField] private GameObject pauseMenuGameObject;
    [SerializeField] private ButtonSelectionManager buttonSelectionManager;

    public override void OnClick()
    {
        Time.timeScale = 0f;
        gameObject.SetActive(false);
        pauseMenuGameObject.SetActive(true);
        buttonSelectionManager.numOfButtons = 0;
    }
    public void Update()
    {
        if(Gamepad.current != null)
        {
            if (Gamepad.current.selectButton.wasPressedThisFrame)
            {
                OnClick();
            }
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnClick();
        }
    }
}
