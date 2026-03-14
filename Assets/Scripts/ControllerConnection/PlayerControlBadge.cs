using System;
using LitMotion;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerControlBadge : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerTeam playerTeam;

    [Header("Graphics")]
    [SerializeField] private CanvasGroup graphics;

    public enum ControlSchemes { WASD, TFGH, IJKL, ArrowKeys, Switch, PlayStation, Xbox }
    private ControlSchemes controlScheme;

    [Header("Ready")]
    [SerializeField] private GameObject readyParent;
    [SerializeField] private TextMeshProUGUI readyText;
    [SerializeField] private GameObject readyKey;
    [SerializeField] private GameObject readyGamepadA;
    [SerializeField] private GameObject readyEnter;
    [SerializeField] private GameObject readyGenericInteractKey;
    [SerializeField] private TextMeshProUGUI readyGenericInteractKeyText;
    [SerializeField] private GameObject readyCheck;

    [Header("Gamepad")]
    [SerializeField] private GameObject gamepadXbox;
    [SerializeField] private GameObject gamepadSwitch;
    [SerializeField] private GameObject gamepadPlaystation;

    [Header("Arrow Keys")]
    [SerializeField] private GameObject arrowKeys;

    [Header("Keys")]
    [SerializeField] private GameObject genericKeys;
    [SerializeField] private TextMeshProUGUI genericInteractKeyText;
    [SerializeField] private TextMeshProUGUI genericUpKeyText;
    [SerializeField] private TextMeshProUGUI genericDownKeyText;
    [SerializeField] private TextMeshProUGUI genericLeftKeyText;
    [SerializeField] private TextMeshProUGUI genericRightKeyText;

    public bool IsReady { get; private set; }
    public event Action ReadyChanged;

    public void Initialize(int playerIndex, ControlSchemes controlScheme)
    {
        this.controlScheme = controlScheme;
        playerTeam.TeamChanged += OnTeamChanged;
        SetupControlScheme();
        ResetReadyText();
        LevelManager.Instance.GameStarted += OnGameStarted;
        LevelManager.Instance.ReturnedToLobby += OnReturnedToLobby;
    }

    private MotionHandle fadeMotionHandle;
    
    private void OnGameStarted()
    {
        Fade(false);
    }

    private void OnReturnedToLobby()
    {
        graphics.gameObject.SetActive(true);
        Fade(true);
    }

    private void Fade(bool fadeIn)
    {
        if (fadeMotionHandle.IsActive())
            fadeMotionHandle.Cancel();
        fadeMotionHandle = FadeInNOutUtility.FadeInOrOut(graphics, LevelManager.Instance.LobbyUIFadeTime, fadeIn, fromCurrentValue: true);
    }

    private void OnTeamChanged()
    {
        TryUnsetReady();
    }

    private void SetupControlScheme()
    {
        switch (controlScheme)
        {
            case ControlSchemes.Switch:
            case ControlSchemes.Xbox:
            case ControlSchemes.PlayStation: SetupControlSchemeGamepad(controlScheme); break;
            case ControlSchemes.ArrowKeys: SetupControlSchemeArrowKeys(); break;
            default: SetupControlSchemeGenericKeys(controlScheme); break;
        }
    }

    private void SetupControlSchemeGamepad(ControlSchemes controlScheme)
    {
        string interactKey = GetInteractKeyString(controlScheme);
        readyKey.SetActive(true);
        readyGamepadA.SetActive(false);
        readyEnter.SetActive(false);
        readyGenericInteractKey.SetActive(true);
        readyGenericInteractKeyText.text = interactKey;
        readyCheck.SetActive(false);

        GameObject selectedGamepad = controlScheme switch
        {
            ControlSchemes.Xbox => gamepadXbox,
            ControlSchemes.Switch => gamepadSwitch,
            ControlSchemes.PlayStation => gamepadPlaystation,
            _ => gamepadXbox,
        };

        gamepadXbox.SetActive(false);
        gamepadSwitch.SetActive(false);
        gamepadPlaystation.SetActive(false);
        selectedGamepad.SetActive(true);
        arrowKeys.SetActive(false);
        genericKeys.SetActive(false);
    }

    private void SetupControlSchemeArrowKeys()
    {
        readyKey.SetActive(true);
        readyGamepadA.SetActive(false);
        readyEnter.SetActive(true);
        readyGenericInteractKey.SetActive(false);
        readyCheck.SetActive(false);

        gamepadXbox.SetActive(false);
        gamepadSwitch.SetActive(false);
        gamepadPlaystation.SetActive(false);
        arrowKeys.SetActive(true);
        genericKeys.SetActive(false);
    }

    private void SetupControlSchemeGenericKeys(ControlSchemes controlScheme)
    {
        string interactKey = GetInteractKeyString(controlScheme);

        readyKey.SetActive(true);
        readyGamepadA.SetActive(false);
        readyEnter.SetActive(false);
        readyGenericInteractKey.SetActive(true);
        readyGenericInteractKeyText.text = interactKey;
        readyCheck.SetActive(false);

        gamepadXbox.SetActive(false);
        gamepadSwitch.SetActive(false);
        gamepadPlaystation.SetActive(false);
        arrowKeys.SetActive(false);
        genericKeys.SetActive(true);

        genericInteractKeyText.text = interactKey;

        string controlSchemeString = controlScheme.ToString();

        genericUpKeyText.text = controlSchemeString.Length > 0 ? controlSchemeString[0].ToString() : "";
        genericLeftKeyText.text = controlSchemeString.Length > 1 ? controlSchemeString[1].ToString() : "";
        genericDownKeyText.text = controlSchemeString.Length > 2 ? controlSchemeString[2].ToString() : "";
        genericRightKeyText.text = controlSchemeString.Length > 3 ? controlSchemeString[3].ToString() : "";
    }

    private string GetInteractKeyString(ControlSchemes controlScheme)
    {
        switch (controlScheme)
        {
            case ControlSchemes.Switch: return "B";
            case ControlSchemes.Xbox: return "A";
            case ControlSchemes.PlayStation: return "X";
            case ControlSchemes.ArrowKeys: return "Enter";
            case ControlSchemes.WASD: return "E";
            case ControlSchemes.TFGH: return "Y";
            case ControlSchemes.IJKL: return "O";
        }

        return "";
    }

    public void OnDisconnect(InputAction.CallbackContext context)
    {
        if (context.performed)
            Destroy(gameObject);
    }

    public void Interact()
    {
        ToggleReady();
    }

    private void ToggleReady()
    {
        if(!IsReady)
        {
            SoundManager.instance.PlaySound("PlayerReady");
        }
        else
        {
            SoundManager.instance.PlaySound("PlayerUnready");
        }
        SetReady(!IsReady);
    }

    private void SetReady(bool ready, bool fireEvent = true)
    {
        IsReady = ready;
        readyCheck.SetActive(ready);
        readyKey.SetActive(!ready);
        if (fireEvent)
        {
            ReadyChanged?.Invoke();
        }
    }

    private void TryUnsetReady()
    {
        if (IsReady) SetUnready();
    }

    public void SetUnready(bool fireEvent = false)
    {
        SetReady(false, fireEvent);
    }

    public void ShowReadyLabel(bool on) => readyParent.SetActive(on);

    public void SetReadyText(string text = "Ready?") => readyText.text = text;

    public void ResetReadyText() => SetReadyText();

    private void OnDisable()
    {
        playerTeam.TeamChanged -= OnTeamChanged;
        LevelManager.Instance.GameStarted -= OnGameStarted;
        LevelManager.Instance.ReturnedToLobby -= OnReturnedToLobby;
    }
}