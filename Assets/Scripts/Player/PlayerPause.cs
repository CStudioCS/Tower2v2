using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPause : MonoBehaviour
{
    [SerializeField] private PlayerInputPoller poller;

    private InputAction pauseAction;

    private InputAction PauseAction
    {
        get
        {
            if (pauseAction == null && poller.LocalPlayerInput != null)
                pauseAction = poller.LocalPlayerInput.actions.FindAction("Gameplay/Pause");

            return pauseAction;
        }
    }


    private void Update()
    {
        if (!LevelManager.InGame)
            return;

        if (PauseAction == null || !PauseAction.WasPressedThisFrame() || PauseMenu.instance == null)
            return;

        PauseMenu.instance.TogglePause();
    }
}
