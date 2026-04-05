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
    [SerializeField] private GameObject networkPanel;
    [SerializeField] private GameObjectFadeIn fader;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private MenuInputHandler inputHandler;

    [Tooltip("Rows of selectables that appear BELOW the dynamic lobby list (e.g., Host, Cancel).")]
    [SerializeField] private List<UIRow> bottomRows = new List<UIRow>();

    [Header("Dynamic Lobbies Area")]
    [SerializeField] private Transform lobbyScrollViewContent;
    [SerializeField] private GameObject lobbyRowPrefab;

    [Header("Scrolling")]
    [SerializeField] private AutoScroller autoScroller;

    // State variables
    private PlayerInput currentPlayer;
    private SettingsMenu originMenu;

    // Stores the dynamically created join buttons
    private List<Selectable> dynamicLobbyButtons = new List<Selectable>();

    /// <summary>
    /// Opens the Network menu, starts the fade, and binds the input handler.
    /// </summary>
    public void Open(PlayerInput player, SettingsMenu origin)
    {
        currentPlayer = player;
        originMenu = origin;

        networkPanel.SetActive(true);

        if (!fader.FadedIn)
            fader.Fade();

        GenerateTestLobbies(15);

        // Build the complete grid and bind controls
        List<UIRow> initialGrid = BuildNavigationGrid();
        inputHandler.Bind(currentPlayer, eventSystem, Close, initialGrid);
    }

    private void GenerateTestLobbies(int amount)
    {
        // Clean first to avoid duplicates if the menu is opened/closed repeatedly
        ClearLobbies();

        for (int i = 0; i < amount; i++)
        {
            AddLobbyToUI();
        }
    }

    /// <summary>
    /// Closes the menu, unbinds the input, and returns control to the previous menu.
    /// </summary>
    public void Close()
    {
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

        // Add dynamic MIDDLE rows (one join button per row)
        foreach (Selectable joinBtn in dynamicLobbyButtons)
        {
            if (joinBtn != null && joinBtn.gameObject.activeInHierarchy && joinBtn.interactable)
            {
                UIRow dynamicRow = new UIRow();
                dynamicRow.items.Add(joinBtn);
                grid.Add(dynamicRow);
            }
        }

        // Add static BOTTOM rows from Inspector
        if (bottomRows != null)
        {
            grid.AddRange(bottomRows);
        }

        return grid;
    }

    /// <summary>
    /// Spawns a new lobby row in the UI and updates navigation.
    /// </summary>
    public void AddLobbyToUI() // Add parameters later (like room name, player count)
    {
        if (lobbyRowPrefab == null || lobbyScrollViewContent == null) return;

        GameObject newRow = Instantiate(lobbyRowPrefab, lobbyScrollViewContent);
        newRow.GetComponentInChildren<TextMeshProUGUI>().text = $"Lobby {dynamicLobbyButtons.Count + 1}";

        Selectable joinButton = newRow.GetComponentInChildren<Selectable>();

        if (joinButton != null)
        {
            dynamicLobbyButtons.Add(joinButton);

            // Rebuild the grid and notify the input handler dynamically
            if (inputHandler.enabled)
            {
                inputHandler.UpdateGrid(BuildNavigationGrid());
            }
        }
    }

    /// <summary>
    /// Clears all dynamically created lobbies from the UI.
    /// </summary>
    public void ClearLobbies()
    {
        foreach (Transform child in lobbyScrollViewContent)
        {
            Destroy(child.gameObject);
        }

        dynamicLobbyButtons.Clear();

        if (inputHandler.enabled)
        {
            inputHandler.UpdateGrid(BuildNavigationGrid());
        }
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

    private void OnEnable()
    {
        if (inputHandler != null)
            inputHandler.OnSelectionChanged += HandleSelectionChanged;
    }

    private void OnDisable()
    {
        if (inputHandler != null)
            inputHandler.OnSelectionChanged -= HandleSelectionChanged;
    }
}