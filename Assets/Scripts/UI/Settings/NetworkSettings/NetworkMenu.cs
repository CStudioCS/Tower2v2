using Fusion;
using System.Collections;
using System.Collections.Generic;
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
    private Dictionary<string, LobbyRowController> existingLobbyRows = new Dictionary<string, LobbyRowController>();
    // Stores the reference to the lobby row representing our current connection
    private LobbyRowController myGameRowController;

    private Coroutine pingCoroutine;

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

            LobbyManager.Instance?.SaveCurrentPositions();
            _ = NetworkManager.Instance.JoinLobby();

            // TODOOO (REFRESH)
            List<UIRow> initialGrid = BuildNavigationGrid();
            inputHandler.Bind(currentPlayer, eventSystem, Close, initialGrid);
        }
    }

    /// <summary>
    /// Closes the menu, unbinds the input, and returns control to the previous menu.
    /// </summary>
    public void Close()
    {
        if (NetworkManager.Instance.IsBusy)
            return;

        isOpen = false;
        if (fader.FadedIn)
            fader.Fade();

        if (originMenu != null && currentPlayer != null)
            originMenu.Open(currentPlayer);

        if (NetworkManager.Instance.Runner != null &&
            !NetworkManager.Instance.IsHosting &&
            !NetworkManager.Instance.IsClient &&
            !NetworkManager.Instance.IsSinglePlayer)
        {
            NetworkManager.Instance.UseSavedPositionsForNextSpawn = true;
            _ = NetworkManager.Instance?.StartNetworkGame(GameMode.Single);
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
        myGameRowController = rowObj.GetComponent<LobbyRowController>();

        UpdateConnectedLobbyRow();
        dynamicLobbyButtons.Add(myGameRowController.ActionButton);

        if (!inputHandler.enabled) 
            inputHandler.Bind(currentPlayer, eventSystem, Close, BuildNavigationGrid());
        else 
            inputHandler.UpdateGrid(BuildNavigationGrid());

        if (pingCoroutine != null) 
            StopCoroutine(pingCoroutine);

        pingCoroutine = StartCoroutine(PingUpdateLoop());
    }

    private IEnumerator PingUpdateLoop()
    {
        while (isOpen && NetworkManager.Instance?.Runner?.IsRunning == true)
        {
            UpdateConnectedLobbyRow();
            yield return new WaitForSeconds(2f);
        }
    }

    /// <summary>
    /// Updates the lobby row that represents the current connection (Host or Client)
    /// </summary>
    private void UpdateConnectedLobbyRow()
    {
        if (myGameRowController == null || NetworkManager.Instance.Runner == null) return;

        bool isHost = NetworkManager.Instance.IsHosting;
        string sessionName = NetworkManager.Instance.CurrentSessionName;
        int displayCount = LobbyManager.Instance != null ? LobbyManager.Instance.TotalPlayers : 1;
        displayCount = Mathf.Max(1, displayCount);

        int pingMs = !isHost ? Mathf.RoundToInt((float)NetworkManager.Instance.Runner.GetPlayerRtt(NetworkManager.Instance.Runner.LocalPlayer) * 1000f) : 0;

        // The row is already instantiated, we just call "Initialize" to overwrite the texts
        myGameRowController.Initialize(
            sessionId: NetworkManager.Instance.CurrentSessionName,
            lobbyName: isHost ? $"Room: {sessionName}" : $"Joined: {sessionName}",
            currentPlayers: displayCount,
            maxPlayers: NetworkManager.Instance.Runner.SessionInfo.MaxPlayers,
            property: $"{pingMs} ms",
            status: isHost ? "Hosting..." : "Connected",
            customButtonText: "Leave",
            isInteractable: true,
            onButtonAction: LeaveGame
        );
    }

    /// <summary>
    /// Updates the lobby rows in the UI and the navigation.
    /// </summary>
    private void RefreshLobbyUI(List<SessionInfo> sessionList)
    {
        if (NetworkManager.Instance.IsHosting || NetworkManager.Instance.IsClient) return;

        bool gridNeedsUpdate = false;
        HashSet<string> currentSessions = new HashSet<string>();

        foreach (SessionInfo session in sessionList)
        {
            if (!session.IsVisible) continue; // Ignore sessions that aren't visible (idk in what case I could use this actually)

            currentSessions.Add(session.Name);

            int realPlayers = session.Properties.TryGetValue("TotalPlayers", out var prop) ? (int)prop : session.PlayerCount;
            string mapName = session.Properties.TryGetValue("MapName", out var mapProp) ? (string)mapProp : "--";

            if (!existingLobbyRows.TryGetValue(session.Name, out LobbyRowController rowController))
            {
                GameObject rowObj = Instantiate(lobbyRowPrefab, lobbyScrollViewContent);
                if (rowObj.TryGetComponent(out rowController))
                {
                    existingLobbyRows.Add(session.Name, rowController);
                    dynamicLobbyButtons.Add(rowController.ActionButton);
                    gridNeedsUpdate = true;
                }
                else continue;
            }

            rowController.Initialize(
                sessionId: session.Name,
                lobbyName: "Game: " + session.Name,
                currentPlayers: realPlayers,
                maxPlayers: session.MaxPlayers,
                property: mapName,
                status: session.IsOpen ? "Waiting..." : "In Progress",
                customButtonText: "Join",
                isInteractable: session.IsOpen && (realPlayers + PlayerInput.all.Count <= session.MaxPlayers),
                onButtonAction: JoinGame
            );
        }

        // Remove the dead bodies
        List<string> keysToRemove = new List<string>();
        foreach (var kvp in existingLobbyRows)
        {
            if (!currentSessions.Contains(kvp.Key))
            {
                dynamicLobbyButtons.Remove(kvp.Value.ActionButton);
                Destroy(kvp.Value.gameObject);
                keysToRemove.Add(kvp.Key);
                gridNeedsUpdate = true; // We removed a button, need to update the grid
            }
        }

        foreach (string k in keysToRemove)
            existingLobbyRows.Remove(k);

        // Tell the MenuInputHandler to rebuild the grid and update navigation since we added new buttons
        if (gridNeedsUpdate && inputHandler.enabled)
            inputHandler.UpdateGrid(BuildNavigationGrid());
    }


    /// <summary>
    /// Clears all dynamically created lobbies from the UI.
    /// </summary>
    public void ClearLobbies()
    {
        if (pingCoroutine != null)
        {
            StopCoroutine(pingCoroutine);
            pingCoroutine = null;
        }

        foreach (Transform child in lobbyScrollViewContent)
            Destroy(child.gameObject);

        dynamicLobbyButtons.Clear();

        existingLobbyRows.Clear();

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

        // Take a picture :)
        LobbyManager.Instance?.SaveCurrentPositions();
        NetworkManager.Instance.UseSavedPositionsForNextSpawn = true;

        // Start the server in Host mode with a random room name
        string roomName = SessionNameGenerator.Generate();
        await NetworkManager.Instance.StartNetworkGame(GameMode.Host, roomName);

        if (this == null || !isOpen) return; // Safety if user closed the menu during await

        uiCanvasGroup.interactable = true; // Unlock UI
        ShowConnectedLobbyRow(); // Show the room we hosted
    }

    private async void LeaveGame(string sessionName)
    {
        uiCanvasGroup.interactable = false; // Lock UI

        // Take a picture :)
        LobbyManager.Instance?.SaveCurrentPositions();
        NetworkManager.Instance.UseSavedPositionsForNextSpawn = true;

        ClearLobbies();
        hostButton.interactable = true;

        // Looking for other lobbies to join after quitting hosting
        await NetworkManager.Instance.JoinLobby();
        if (this == null || !isOpen) return; // Safety if user closed the menu during await

        uiCanvasGroup.interactable = true; // Unlock UI
    }

    public async void JoinGame(string roomName)
    {
        uiCanvasGroup.interactable = false; // Lock UI

        // Take a picture :)
        LobbyManager.Instance?.SaveCurrentPositions();
        NetworkManager.Instance.UseSavedPositionsForNextSpawn = true;

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

        NetworkManager.OnPlayersCountChanged += UpdateConnectedLobbyRow;
        NetworkManager.OnUnexpectedDisconnect += Close;
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnSelectionChanged -= HandleSelectionChanged;

        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnSessionListUpdatedEvent -= RefreshLobbyUI;

        NetworkManager.OnPlayersCountChanged -= UpdateConnectedLobbyRow;
        NetworkManager.OnUnexpectedDisconnect -= Close;
    }
}