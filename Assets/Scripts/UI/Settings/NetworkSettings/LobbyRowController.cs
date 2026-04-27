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
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI propertyText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button joinButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private string sessionName;

    public string LobbyName { get => lobbyNameText.text; set => lobbyNameText.text = value; }
    public string PlayerCount { get => playerCountText.text; set => playerCountText.text = value; }
    public string Property { get => propertyText.text; set => propertyText.text = value; }
    public string Status { get => statusText.text; set => statusText.text = value; }
    public Button ActionButton => joinButton;

    /// <summary>
    /// Configures the entire row in one go and binds the join button.
    /// </summary>
    public void Initialize(string sessionId, string lobbyName, int currentPlayers, int maxPlayers, string property, 
        string status, string customButtonText, bool isInteractable, Action<string> onButtonAction)
    {
        this.sessionName = sessionId;

        LobbyName = lobbyName;
        PlayerCount = $"{currentPlayers}/{maxPlayers}";
        Property = property;
        Status = status;

        if (buttonText != null)
            buttonText.text = customButtonText;

        joinButton.interactable = isInteractable;

        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => onButtonAction?.Invoke(this.sessionName));
    }
}