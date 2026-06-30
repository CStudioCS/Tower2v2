using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Controls a single row in the network menu.
/// </summary>
public class LobbyRowController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private EditableText editableLobbyName;
    [SerializeField] private EditableText editablePlayerName;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI propertyText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button joinButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private string sessionName;

    public string LobbyName { get => editableLobbyName.DisplayText.text; set => editableLobbyName.DisplayText.text = value; }
    public string PlayerCount { get => playerCountText.text; set => playerCountText.text = value; }
    public string Property { get => propertyText.text; set => propertyText.text = value; }
    public string Status { get => statusText.text; set => statusText.text = value; }
    public Button ActionButton => joinButton;

    /// <summary>
    /// Configures the entire row in one go and binds the join button.
    /// </summary>
    public void Initialize(string sessionId, string lobbyName, int currentPlayers, int maxPlayers, string property, 
        string status, string customButtonText, bool isInteractable, Action<string> onButtonAction, 
        string initialSessionName,
        string initialPlayerName,
        MenuInputHandler menuHandler = null,
        bool isSessionNameEditable = false,
        bool showPlayerName = false)
    {
        this.sessionName = sessionId;

        PlayerCount = $"{currentPlayers}/{maxPlayers}";
        Property = property;
        Status = status;

        buttonText.text = customButtonText;

        joinButton.interactable = isInteractable;
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => onButtonAction?.Invoke(this.sessionName));

        editableLobbyName.Initialize(menuHandler, initialSessionName, isSessionNameEditable);
        LobbyName = lobbyName;

        editableLobbyName.OnValueChanged -= OnSessionNameEdited;
        editableLobbyName.OnValueChanged += OnSessionNameEdited;

        editablePlayerName.gameObject.SetActive(showPlayerName);

        if (showPlayerName)
        {
            editablePlayerName.Initialize(menuHandler, initialPlayerName);
            editablePlayerName.OnValueChanged -= OnPlayerNameEdited;
            editablePlayerName.OnValueChanged += OnPlayerNameEdited;
        }
    }

    private void OnSessionNameEdited(string newName) => LobbyManager.Instance?.SetCustomSessionName(newName);
    private void OnPlayerNameEdited(string newName) => LobbyManager.Instance?.SetLocalPlayerName(newName);

    private void OnDestroy()
    {
        editableLobbyName.OnValueChanged -= OnSessionNameEdited;
        editablePlayerName.OnValueChanged -= OnPlayerNameEdited;
    }
}