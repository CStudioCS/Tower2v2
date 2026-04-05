using Fusion;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manages the Network Menu UI, handling dynamic lobby lists and grid-based gamepad navigation.
/// Uses UIRow to allow Inspector-driven layouts for static top/bottom elements.
/// </summary>
public class NetworkMenu : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private CanvasGroup uiCanvasGroup;
    [SerializeField] private GameObject networkPanel;
    [SerializeField] private GameObjectFadeIn fader;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private MenuInputHandler inputHandler;

    [Tooltip("Rows of selectables that appear BELOW the dynamic lobby list (e.g., Host, Cancel).")]
    [SerializeField] private List<UIRow> bottomRows = new List<UIRow>();
    [SerializeField] private Button hostButton;

    [Header("Dynamic Lobbies Area")]
    [SerializeField] private Transform lobbyScrollViewContent;
    [SerializeField] private GameObject lobbyRowPrefab;

    [Header("Scrolling")]
    [SerializeField] private AutoScroller autoScroller;

    // State variables
    private PlayerInput currentPlayer;
    private SettingsMenu originMenu;
    private bool isOpen = false;

    // Stores the dynamically created join buttons
    private List<Selectable> dynamicLobbyButtons = new List<Selectable>();

    /// <summary>
    /// Opens the Network menu, starts the fade, and binds the input handler.
    /// </summary>
    public void Open(PlayerInput player, SettingsMenu origin)
    {
        isOpen = true;
        currentPlayer = player;
        originMenu = origin;

        networkPanel.SetActive(true);

        if (!fader.FadedIn)
            fader.Fade();

        if (NetworkManager.Instance.IsBusy)
            uiCanvasGroup.interactable = false;

        // If we are already connected (Host OR Client), show our current room
        else if (NetworkManager.Instance.IsHosting || NetworkManager.Instance.IsClient)
        {
            uiCanvasGroup.interactable = true;
            ShowConnectedLobbyRow();
        }

        else
        {
            // If not hosting, clear lobbies, enable the Host button, and join the global lobby
            ClearLobbies();
            uiCanvasGroup.interactable = true;
            hostButton.interactable = true;
            _ = NetworkManager.Instance.JoinLobby();

            // It will update automatically when OnSessionListUpdatedEvent fires.
            List<UIRow> initialGrid = BuildNavigationGrid();
            inputHandler.Bind(currentPlayer, eventSystem, Close, initialGrid);
        }
    }

    /// <summary>
    /// Closes the menu, unbinds the input, and returns control to the previous menu.
    /// </summary>
    public void Close()
    {
        isOpen = false;
        if (fader.FadedIn)
            fader.Fade();

        if (originMenu != null && currentPlayer != null)
        {
            originMenu.Open(currentPlayer);
        }
    }

    /// <summary>
    /// Assembles the 2D grid of selectables by stitching together top rows, 
    /// dynamic lobby rows, and bottom rows.
    /// </summary>
    /// <returns>The combined layout grid.</returns>
    private List<UIRow> BuildNavigationGrid()
    {
        List<UIRow> grid = new List<UIRow>();

        // Add dynamic middle rows (one join button per row)
        foreach (Selectable joinBtn in dynamicLobbyButtons)
        {
            if (joinBtn != null && joinBtn.gameObject.activeInHierarchy && joinBtn.interactable)
            {
                UIRow dynamicRow = new UIRow();
                dynamicRow.items.Add(joinBtn);
                grid.Add(dynamicRow);
            }
        }

        // Add static bottom rows from Inspector
        if (bottomRows != null)
            grid.AddRange(bottomRows);

        return grid;
    }

    private void ShowConnectedLobbyRow()
    {
        ClearLobbies();
        hostButton.interactable = false; // Can't host if already connected

        GameObject rowObj = Instantiate(lobbyRowPrefab, lobbyScrollViewContent);
        LobbyRowController rowController = rowObj.GetComponent<LobbyRowController>();

        bool isHost = NetworkManager.Instance.IsHosting;

        rowController.Initialize(
            sessionId: NetworkManager.Instance.CurrentSessionName,
            lobbyName: isHost ? "Your Hosted Game" : "Joined Game",
            currentPlayers: 1, // Will be replaced by live count later
            maxPlayers: 4,
            ping: 0,
            status: isHost ? "Hosting..." : "Connected",
            customButtonText: "Leave",
            isInteractable: true,
            onButtonAction: LeaveGame
        );

        dynamicLobbyButtons.Add(rowController.ActionButton);

        if (!inputHandler.enabled) 
            inputHandler.Bind(currentPlayer, eventSystem, Close, BuildNavigationGrid());
        else 
            inputHandler.UpdateGrid(BuildNavigationGrid());
    }

    /// <summary>
    /// Updates the lobby rows in the UI and the navigation.
    /// </summary>
    private void RefreshLobbyUI(List<SessionInfo> sessionList)
    {
        if (NetworkManager.Instance.IsHosting || NetworkManager.Instance.IsClient) return;

        ClearLobbies();

        foreach (SessionInfo session in sessionList)
        {
            // Ignore sessions that aren't visible or open for joining
            if (!session.IsVisible || !session.IsOpen) continue;

            GameObject rowObj = Instantiate(lobbyRowPrefab, lobbyScrollViewContent);
            
            if (rowObj.TryGetComponent(out LobbyRowController rowController))
            {
                int realPlayers = session.Properties.TryGetValue("TotalPlayers", out var prop) ? (int)prop : session.PlayerCount;

                rowController.Initialize(
                    sessionId: session.Name,
                    lobbyName: "Game: " + session.Name,
                    currentPlayers: realPlayers,
                    maxPlayers: session.MaxPlayers,
                    ping: 45,
                    status: session.IsOpen ? "Waiting..." : "In Progress",
                    customButtonText: "Join",
                    isInteractable: realPlayers < session.MaxPlayers,
                    onButtonAction: JoinGame
                );

                dynamicLobbyButtons.Add(rowController.ActionButton);
            }
        }

        // Tell the MenuInputHandler to rebuild the grid and update navigation since we added new buttons
        if (inputHandler.enabled) 
            inputHandler.UpdateGrid(BuildNavigationGrid());
    }

    /// <summary>
    /// Clears all dynamically created lobbies from the UI.
    /// </summary>
    public void ClearLobbies()
    {
        foreach (Transform child in lobbyScrollViewContent)
            Destroy(child.gameObject);

        dynamicLobbyButtons.Clear();

        if (inputHandler.enabled) 
            inputHandler.UpdateGrid(BuildNavigationGrid());
    }

    /// <summary>
    /// Triggered by the InputHandler. We find the parent Row and scroll to it.
    /// </summary>
    private void HandleSelectionChanged(GameObject selectedObj)
    {
        if (autoScroller == null || selectedObj == null) return;

        UnityEngine.UI.HorizontalLayoutGroup rowGroup = selectedObj.GetComponentInParent<UnityEngine.UI.HorizontalLayoutGroup>();

        if (rowGroup != null)
            autoScroller.ScrollToTarget(rowGroup.GetComponent<RectTransform>());
        else
            autoScroller.ScrollToTarget(selectedObj.GetComponent<RectTransform>());
    }

    public async void HostGame()
    {
        uiCanvasGroup.interactable = false; // Lock UI

        hostButton.interactable = false;
        await NetworkManager.Instance.Disconnect();

        // Start the server in Host mode with a random room name
        string roomName = "Lobby_" + UnityEngine.Random.Range(1000, 9999);
        await NetworkManager.Instance.StartNetworkGame(GameMode.Host, roomName);

        if (this == null || !isOpen) return; // Safety if user closed the menu during await

        uiCanvasGroup.interactable = true; // Unlock UI
        ShowConnectedLobbyRow(); // Show the room we hosted
    }

    private async void LeaveGame(string sessionName)
    {
        uiCanvasGroup.interactable = false; // Lock UI
        await NetworkManager.Instance.Disconnect();

        if (this == null || !isOpen) return;

        uiCanvasGroup.interactable = true; // Unlock UI
        ClearLobbies();
        hostButton.interactable = true;

        // Looking for other lobbies to join after quitting hosting
        _ = NetworkManager.Instance.JoinLobby();
    }

    public async void JoinGame(string roomName)
    {
        uiCanvasGroup.interactable = false; // Lock UI

        await NetworkManager.Instance.Disconnect();
        await NetworkManager.Instance.StartNetworkGame(GameMode.Client, roomName);

        if (this == null || !isOpen) return; // Safety if user closed the menu during await

        uiCanvasGroup.interactable = true; // Unlock UI
        ShowConnectedLobbyRow(); // Show the room we joined
    }

    private void OnEnable()
    {
        if (inputHandler != null)
            inputHandler.OnSelectionChanged += HandleSelectionChanged;

        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnSessionListUpdatedEvent += RefreshLobbyUI;
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnSelectionChanged -= HandleSelectionChanged;

        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnSessionListUpdatedEvent -= RefreshLobbyUI;
    }
}