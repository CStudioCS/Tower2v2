using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPause : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    private InputAction pauseAction;

    private void Awake() => pauseAction = playerInput.actions.FindAction("Gameplay/Pause");

    private void Update()
    {
        if (!LevelManager.InGame)
            return;

        if (!pauseAction.WasPressedThisFrame())
            return;

        RequestPauseToggle();
    }

    private void RequestPauseToggle()
    {
        if (PauseMenu.instance == null)
            return;

        PauseMenu.instance.TogglePause();
    }
}
