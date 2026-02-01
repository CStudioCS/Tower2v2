using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerControlBadge : MonoBehaviour
{
    private Transform leftTeamContainer;
    private Transform rightTeamContainer;
    
    [Header("Ready")]
    [SerializeField] private GameObject readyKey;
    [SerializeField] private GameObject readyGamepadA;
    [SerializeField] private GameObject readyCtrl;
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
    
    private Transform _uiContainer;

    public void Setup(int playerIndex, string controlScheme, Transform leftTeamContainer, Transform rightTeamContainer)
    {
        // if (_uiContainer == null)
        // {
        //     GameObject containerObject = GameObject.FindGameObjectWithTag("LobbyContainer");
        //     if (containerObject != null)
        //     {
        //         _uiContainer = containerObject.transform;
        //     }
        // }
        //
        // if (_uiContainer != null)
        // {
        //     transform.SetParent(_uiContainer, false);
        // }
        
        this.leftTeamContainer = leftTeamContainer;
        this.rightTeamContainer = rightTeamContainer;

        SetTeamLeft();
        SetupControlScheme(controlScheme);
    }

    private void SetTeamLeft() => transform.SetParent(leftTeamContainer, false);
    private void SetTeamRight() => transform.SetParent(rightTeamContainer, false);
    
    private void SetupControlScheme(string controlScheme)
    {
        switch (controlScheme)
        {
            case "Gamepad": SetupControlSchemeGamepad(); break;
            case "ArrowKeys": SetupControlSchemeArrowKeys(); break;
            default: SetupControlSchemeGenericKeys(controlScheme); break;
        }
    }

    private void SetupControlSchemeGamepad()
    {
        readyKey.SetActive(true);
        readyGamepadA.SetActive(true);
        readyCtrl.SetActive(false);
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
        readyCtrl.SetActive(true);
        readyGenericInteractKey.SetActive(false);
        readyCheck.SetActive(false);
        
        gamepad.SetActive(false);
        arrowKeys.SetActive(true);
        genericKeys.SetActive(false);
    }

    private void SetupControlSchemeGenericKeys(string controlScheme)
    {
        string interactKey = GetInteractKeyString(controlScheme);
        
        readyKey.SetActive(true);
        readyGamepadA.SetActive(false);
        readyCtrl.SetActive(false);
        readyGenericInteractKey.SetActive(true);
        readyGenericInteractKeyText.text = interactKey;
        readyCheck.SetActive(false);
        
        gamepad.SetActive(false);
        arrowKeys.SetActive(false);
        genericKeys.SetActive(true);

        genericInteractKeyText.text = interactKey;
        genericUpKeyText.text = GetUpKeyString(controlScheme);
        genericLeftKeyText.text = GetLeftKeyString(controlScheme);
        genericDownKeyText.text = controlScheme[2].ToString();
        genericRightKeyText.text = controlScheme[3].ToString();
    }

    private string GetInteractKeyString(string controlScheme)
    {
        switch (controlScheme)
        {
            case "Gamepad": return "A";
            case "ArrowKeys": return "Ctrl";
            case "WASD": return "E";
            case "TFGH": return "Y";
            case "IJKL": return "O";
        }
        return "";
    }
    
    private string GetUpKeyString(string controlScheme)
    {
        switch (controlScheme)
        {
            case "Gamepad": return "";
            case "ArrowKeys": return "Up";
            case "WASD": return "Z";
            case "TFGH": return "T";
            case "IJKL": return "I";
        }
        return "";
    }
    
    private string GetLeftKeyString(string controlScheme)
    {
        switch (controlScheme)
        {
            case "Gamepad": return "";
            case "ArrowKeys": return "Left";
            case "WASD": return "Q";
            case "TFGH": return "F";
            case "IJKL": return "J";
        }
        return "";
    }

    public void OnDisconnect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Destroy(gameObject);
        }
    }
}