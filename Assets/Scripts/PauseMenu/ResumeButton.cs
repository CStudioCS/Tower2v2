using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ResumeButton : NoActionButton
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pauseMenu;

    public override void OnClick()
    {
        pauseMenu.SetActive(false);
        pauseButton.gameObject.SetActive(true);
        Time.timeScale = 1.0f;
    }

    public void Update()
    {
        if (Gamepad.current != null)
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
