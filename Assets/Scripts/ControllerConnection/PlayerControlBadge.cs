using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControlBadge : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerTeam playerTeam;
    
    [Header("Color")]
    [SerializeField] private Graphic[] teamColorGraphics;
    
    [Header("Graphics")]
    [SerializeField] private GameObject graphics;

    public enum ControlSchemes { WASD, TFGH, IJKL, ArrowKeys, Gamepad }

    [Header("Ready")]
    [SerializeField] private GameObject readyKey;
    [SerializeField] private GameObject readyGamepadA;
    [SerializeField] private GameObject readyEnter;
    [SerializeField] private GameObject readyGenericInteractKey;
    [SerializeField] private TextMeshProUGUI readyGenericInteractKeyText;
    [SerializeField] private GameObject readyCheck;

    [Header("Gamepad")]
    [SerializeField] private GameObject gamepad;
    
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
        playerTeam.TeamChanged += OnTeamChanged;
        UpdateColor();
        SetupControlScheme(controlScheme);
        LevelManager.Instance.GameStarted += OnGameStarted;
        LevelManager.Instance.GameEnded += OnGameEnded;

#if DEBUG
        if (LobbyManager.Instance.DebugMode)
        {
            //quick and dirty but it's debug
            if (GameStartManager.Instance.PlayerCount % 2 == 0)
                playerTeam.SetTeam(PlayerTeam.Team.Left);
            else
                playerTeam.SetTeam(PlayerTeam.Team.Right);
            ToggleReady();
        }
#endif
    }

    private void OnGameStarted() => graphics.SetActive(false);

    private void OnGameEnded() => graphics.SetActive(true);

    private void OnTeamChanged()
    {
        TryUnsetReady();
        UpdateColor();
    }

    private void UpdateColor() => ChangeColor(playerTeam.TeamColors[playerTeam.CurrentTeam]);
    private void ChangeColor(Color color)
    {
        foreach (Graphic graphic in teamColorGraphics)
        {
            graphic.color = color;
        }
    }
    
    private void SetupControlScheme(ControlSchemes controlScheme)
    {
        switch (controlScheme)
        {
            case ControlSchemes.Gamepad: SetupControlSchemeGamepad(); break;
            case ControlSchemes.ArrowKeys: SetupControlSchemeArrowKeys(); break;
            default: SetupControlSchemeGenericKeys(controlScheme); break;
        }
    }

    private void SetupControlSchemeGamepad()
    {
        readyKey.SetActive(true);
        readyGamepadA.SetActive(true);
        readyEnter.SetActive(false);
        readyGenericInteractKey.SetActive(false);
        readyCheck.SetActive(false);
        
        gamepad.SetActive(true);
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
        
        gamepad.SetActive(false);
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
        
        gamepad.SetActive(false);
        arrowKeys.SetActive(false);
        genericKeys.SetActive(true);

        genericInteractKeyText.text = interactKey;
        genericUpKeyText.text = GetUpKeyString(controlScheme);
        genericLeftKeyText.text = GetLeftKeyString(controlScheme);
        genericDownKeyText.text = controlScheme.ToString()[2].ToString();
        genericRightKeyText.text = controlScheme.ToString()[3].ToString();
    }

    private string GetInteractKeyString(ControlSchemes controlScheme)
    {
        switch (controlScheme)
        {
            case ControlSchemes.Gamepad: return "A";
            case ControlSchemes.ArrowKeys: return "Enter";
            case ControlSchemes.WASD: return "E";
            case ControlSchemes.TFGH: return "Y";
            case ControlSchemes.IJKL: return "O";
        }

        Debug.LogError($"Control scheme {controlScheme} is not correctly handled");
        return "";
    }
    
    private string GetUpKeyString(ControlSchemes controlScheme)
    {
        switch (controlScheme)
        {
            case ControlSchemes.Gamepad: return "";
            case ControlSchemes.ArrowKeys: return "Up";
            case ControlSchemes.WASD: return "Z";
            case ControlSchemes.TFGH: return "T";
            case ControlSchemes.IJKL: return "I";
        }

        Debug.LogError($"Control scheme {controlScheme} is not correctly handled");
        return "";
    }
    
    private string GetLeftKeyString(ControlSchemes controlScheme)
    {
        switch (controlScheme)
        {
            case ControlSchemes.Gamepad: return "";
            case ControlSchemes.ArrowKeys: return "Left";
            case ControlSchemes.WASD: return "Q";
            case ControlSchemes.TFGH: return "F";
            case ControlSchemes.IJKL: return "J";
        }

        Debug.LogError($"Control scheme {controlScheme} is not correctly handled");
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

    private void ToggleReady() => SetReady(!IsReady);
    private void SetReady(bool ready, bool fireEvent = true)
    {
        IsReady = ready;
        readyCheck.SetActive(ready);
        readyKey.SetActive(!ready);
        if (fireEvent)
            ReadyChanged?.Invoke();
    }

    private void TryUnsetReady()
    {
        if (IsReady) SetUnready();
    }

    public void SetUnready()
    {
        SetReady(false, false);
    }

    private void OnDisable()
    {
        playerTeam.TeamChanged -= OnTeamChanged;
        LevelManager.Instance.GameStarted -= OnGameStarted;
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }
}