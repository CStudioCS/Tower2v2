using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Generic 2D UI navigation handler driven by specific player input actions.
/// Uses a grid system (List of UIRows) to handle complex layouts dynamically.
/// </summary>
public class MenuInputHandler : MonoBehaviour
{
    [Header("Navigation Settings")]
    [Tooltip("Delay before repeated navigation when holding the stick")]
    [SerializeField] private float repeatDelay = 0.5f;
    [Tooltip("Rate of repeated navigation while holding the stick")]
    [SerializeField] private float repeatRate = 0.1f;
    [Tooltip("Stick deadzone for navigation")]
    [SerializeField] private float deadzone = 0.5f;
    [Tooltip("Minimum mouse delta (pixels) to count as intentional mouse movement")]
    [SerializeField] private float mouseMovementThreshold = 2f;

    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private EventSystem eventSystem;

    // --- 2D Grid System ---
    private List<UIRow> grid = new List<UIRow>();
    private int rowIndex = 0;
    private int colIndex = 0;

    private float nextMoveTime;
    private Vector2 lastDirection;
    private PointerEventData cachedPointerData;

    private bool gamepadHasControl;
    private Vector2 lastMousePosition;
    private bool wasNavigatingLastFrame;

    private InputSystemUIInputModule uiModule;
    private bool hadMoveAction, hadSubmitAction, hadCancelAction;
    private EventSystem disabledExtraEventSystem;

    private float inputCooldownTime;

    // --- Events ---
    public event Action<GameObject> OnSelectionChanged;
    private Action onCancelCallback;

    /// <summary>
    /// Delegate allowing external scripts to consume navigation inputs (e.g., Sliders).
    /// Returns true if the input was consumed.
    /// </summary>
    public delegate bool CustomNavigationHandler(GameObject selectedObj, Vector2 input);
    public event CustomNavigationHandler OnCustomNavigation;

    public void Bind(PlayerInput playerInput, EventSystem targetEventSystem, Action onCancel, List<UIRow> navigationGrid, GameObject defaultSelection = null)
    {
        inputCooldownTime = Time.unscaledTime + 0.2f;

        if (enabled && navigateAction != null)
        {
            onCancelCallback = onCancel;
            grid = navigationGrid;

            SyncIndicesToTarget(defaultSelection);
            ClampIndices();

            if (gamepadHasControl) SelectCurrent();
            else eventSystem.SetSelectedGameObject(null);

            return;
        }

        onCancelCallback = onCancel;
        grid = navigationGrid;

        navigateAction = playerInput.actions.FindAction("UI/Navigate");
        submitAction = playerInput.actions.FindAction("UI/Submit");
        cancelAction = playerInput.actions.FindAction("UI/Cancel");

        nextMoveTime = 0f;
        lastDirection = Vector2.zero;
        wasNavigatingLastFrame = false;
        lastMousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;

        eventSystem = EventSystem.current != null ? EventSystem.current : targetEventSystem;
        disabledExtraEventSystem = null;

        if (targetEventSystem != null && targetEventSystem != eventSystem)
        {
            targetEventSystem.gameObject.SetActive(false);
            disabledExtraEventSystem = targetEventSystem;
        }

        cachedPointerData = new PointerEventData(eventSystem);

        uiModule = FindAnyObjectByType<InputSystemUIInputModule>();
        if (uiModule != null)
        {
            hadMoveAction = DisableModuleAction(uiModule.move);
            hadSubmitAction = DisableModuleAction(uiModule.submit);
            hadCancelAction = DisableModuleAction(uiModule.cancel);
            uiModule.enabled = false;
        }

        eventSystem.SetSelectedGameObject(null);
        ClearAllPointerState();

        gamepadHasControl = true;

        SyncIndicesToTarget(defaultSelection);
        ClampIndices();

        SelectCurrent();
        enabled = true;
    }

    public void Unbind()
    {
        if (uiModule != null)
        {
            RestoreModuleAction(uiModule.move, hadMoveAction);
            RestoreModuleAction(uiModule.submit, hadSubmitAction);
            RestoreModuleAction(uiModule.cancel, hadCancelAction);
            uiModule.enabled = true;
            uiModule = null;
        }

        if (disabledExtraEventSystem != null)
        {
            disabledExtraEventSystem.gameObject.SetActive(true);
            disabledExtraEventSystem = null;
        }

        navigateAction = null; submitAction = null; cancelAction = null;
        eventSystem = null; grid = null;
        cachedPointerData = null;
        enabled = false;
    }

    public void UpdateGrid(List<UIRow> newGrid)
    {
        grid = newGrid;
        if (gamepadHasControl)
        {
            ClampIndices();
            SelectCurrent();
        }
    }

    private void Update()
    {
        if (eventSystem == null) return;

        DetectInputSwitch();

        if (gamepadHasControl)
        {
            GameObject expected = GetCurrentExpectedGameObject();
            if (expected != null && eventSystem.currentSelectedGameObject != expected)
                eventSystem.SetSelectedGameObject(expected);

            ClearAllPointerState();
        }

        HandleNavigation();
        HandleSubmit();
        HandleCancel();
    }

    private void HandleNavigation()
    {
        if (Time.unscaledTime < inputCooldownTime) return;
        if (!gamepadHasControl || grid == null || grid.Count == 0) return;

        Vector2 input = navigateAction.ReadValue<Vector2>();

        if (input.magnitude < deadzone)
        {
            lastDirection = Vector2.zero;
            nextMoveTime = 0f;
            return;
        }

        if (Time.unscaledTime < nextMoveTime) return;

        GameObject currentObj = GetCurrentExpectedGameObject();
        bool handledExternally = false;

        // --- Event Delegation: Check if an external script wants to consume this input ---
        if (OnCustomNavigation != null)
        {
            foreach (CustomNavigationHandler handler in OnCustomNavigation.GetInvocationList())
            {
                if (handler.Invoke(currentObj, input))
                {
                    handledExternally = true;
                    break;
                }
            }
        }

        // If an external script (like SliderController) consumed the input, handle repeat delay and abort grid movement
        if (handledExternally)
        {
            bool newDirection = lastDirection == Vector2.zero || Vector2.Dot(input.normalized, lastDirection.normalized) < 0.5f;
            nextMoveTime = Time.unscaledTime + (newDirection ? repeatDelay : repeatRate);
            lastDirection = input;
            return;
        }

        // --- Standard 2D Grid Navigation ---
        int vertical = Mathf.Abs(input.y) >= Mathf.Abs(input.x) ? (input.y > 0 ? -1 : 1) : 0;
        int horizontal = Mathf.Abs(input.x) > Mathf.Abs(input.y) ? (input.x > 0 ? 1 : -1) : 0;

        bool moved = MoveToNextValid(vertical, horizontal);

        if (moved)
        {
            bool isNewDirection = lastDirection == Vector2.zero || Vector2.Dot(input.normalized, lastDirection.normalized) < 0.5f;
            nextMoveTime = Time.unscaledTime + (isNewDirection ? repeatDelay : repeatRate);
            lastDirection = input;
            SelectCurrent();
        }
    }

    private void ClampIndices()
    {
        if (grid.Count == 0) return;

        rowIndex = Mathf.Clamp(rowIndex, 0, grid.Count - 1);

        int rowLength = grid[rowIndex].items.Count;
        if (rowLength == 0) colIndex = 0;
        else colIndex = Mathf.Clamp(colIndex, 0, rowLength - 1);
    }

    private void HandleSubmit()
    {
        if (Time.unscaledTime < inputCooldownTime) return;
        if (!gamepadHasControl || submitAction == null || !submitAction.WasPressedThisFrame()) return;

        GameObject target = GetCurrentExpectedGameObject();
        if (target != null)
            ExecuteEvents.Execute(target, new BaseEventData(eventSystem), ExecuteEvents.submitHandler);
    }

    private void HandleCancel()
    {
        if (Time.unscaledTime < inputCooldownTime) return;
        if (cancelAction == null || !cancelAction.WasPressedThisFrame()) return;

        onCancelCallback?.Invoke();
    }

    private GameObject GetCurrentExpectedGameObject()
    {
        if (grid == null || grid.Count == 0 || rowIndex < 0 || rowIndex >= grid.Count) return null;

        UIRow row = grid[rowIndex];
        if (row.items == null || row.items.Count == 0 || colIndex < 0 || colIndex >= row.items.Count) return null;

        Selectable sel = row.items[colIndex];
        return sel != null ? sel.gameObject : null;
    }

    private void SelectCurrent()
    {
        GameObject expected = GetCurrentExpectedGameObject();
        if (expected != null)
        {
            eventSystem.SetSelectedGameObject(expected);
            OnSelectionChanged?.Invoke(expected);
        }
    }

    private void SyncIndicesToTarget(GameObject target)
    {
        rowIndex = 0;
        colIndex = 0;
        if (target == null || grid == null) return;

        for (int r = 0; r < grid.Count; r++)
        {
            for (int c = 0; c < grid[r].items.Count; c++)
            {
                if (grid[r].items[c] != null && grid[r].items[c].gameObject == target)
                {
                    rowIndex = r;
                    colIndex = c;
                    return;
                }
            }
        }
    }

    private void DetectInputSwitch()
    {
        bool mouseActed = false;
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if ((mousePos - lastMousePosition).sqrMagnitude > mouseMovementThreshold * mouseMovementThreshold || Mouse.current.leftButton.wasPressedThisFrame)
                mouseActed = true;
            lastMousePosition = mousePos;
        }

        bool isNavigating = navigateAction != null && navigateAction.ReadValue<Vector2>().magnitude >= deadzone;
        bool gamepadActed = isNavigating && !wasNavigatingLastFrame;
        wasNavigatingLastFrame = isNavigating;

        if (!gamepadActed && submitAction != null && submitAction.WasPressedThisFrame()) gamepadActed = true;
        if (!gamepadActed && cancelAction != null && cancelAction.WasPressedThisFrame()) gamepadActed = true;

        if (mouseActed && !gamepadActed && gamepadHasControl)
        {
            gamepadHasControl = false;
            if (uiModule != null) uiModule.enabled = true;
            eventSystem.SetSelectedGameObject(null);
        }

        if (gamepadActed && !mouseActed && !gamepadHasControl)
        {
            gamepadHasControl = true;
            if (uiModule != null) uiModule.enabled = false;
            SyncIndexToCurrentSelection();
            ClearAllPointerState();
            SelectCurrent();
        }
    }

    private void SyncIndexToCurrentSelection()
    {
        GameObject current = eventSystem.currentSelectedGameObject;
        if (current == null || grid == null) return;

        for (int r = 0; r < grid.Count; r++)
        {
            for (int c = 0; c < grid[r].items.Count; c++)
            {
                if (grid[r].items[c] != null && grid[r].items[c].gameObject == current)
                {
                    rowIndex = r;
                    colIndex = c;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Checks if a Selectable is active in the scene and interactable.
    /// </summary>
    private bool IsValidTarget(Selectable sel)
    {
        return sel != null && sel.gameObject.activeInHierarchy && sel.interactable;
    }

    /// <summary>
    /// Searches for the next valid, interactable target in the given direction.
    /// Applies smart logic: if moving vertically and the target is disabled, 
    /// it looks for a valid neighbor in the same row before jumping to the next row.
    /// </summary>
    private bool MoveToNextValid(int dirRow, int dirCol)
    {
        if (grid.Count == 0) return false;

        int startRow = rowIndex;
        int startCol = colIndex;

        int currentRow = rowIndex;
        int currentCol = colIndex;

        int totalRows = grid.Count;
        int maxAttempts = 100; // Failsafe anti-boucle infinie
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            attempts++;

            if (dirRow != 0) // -- Vertical Navigation --
            {
                currentRow += dirRow;

                // Vertical Wrap
                if (currentRow < 0) currentRow = totalRows - 1;
                if (currentRow >= totalRows) currentRow = 0;

                int rowLength = grid[currentRow].items.Count;
                if (rowLength == 0) continue; // Skip empty rows

                currentCol = Mathf.Clamp(colIndex, 0, rowLength - 1);

                // If the direct target is disabled, apply the user's rule:
                // Check right, then check left, before giving up on this row.
                if (!IsValidTarget(grid[currentRow].items[currentCol]))
                {
                    bool foundInRow = false;

                    // 1. Check right
                    for (int c = currentCol + 1; c < rowLength; c++)
                    {
                        if (IsValidTarget(grid[currentRow].items[c]))
                        {
                            currentCol = c;
                            foundInRow = true;
                            break;
                        }
                    }

                    // 2. Check left if nothing was found on the right
                    if (!foundInRow)
                    {
                        for (int c = currentCol - 1; c >= 0; c--)
                        {
                            if (IsValidTarget(grid[currentRow].items[c]))
                            {
                                currentCol = c;
                                foundInRow = true;
                                break;
                            }
                        }
                    }

                    // If absolutely nothing is valid in this row, continue to the next row (below/above)
                    if (!foundInRow) continue;
                }
            }
            else if (dirCol != 0) // -- Horizontal Navigation --
            {
                int rowLength = grid[currentRow].items.Count;
                if (rowLength == 0) return false;

                currentCol += dirCol;

                // Horizontal Wrap
                if (currentCol < 0) currentCol = rowLength - 1;
                if (currentCol >= rowLength) currentCol = 0;

                // If the target is disabled, the while loop will simply run again and skip to the next!
                if (!IsValidTarget(grid[currentRow].items[currentCol]))
                {
                    continue;
                }
            }

            // We found a valid target!
            if (IsValidTarget(grid[currentRow].items[currentCol]))
            {
                rowIndex = currentRow;
                colIndex = currentCol;
                return true;
            }

            // If we wrapped all the way around and nothing is available
            if (currentRow == startRow && currentCol == startCol) break;
        }

        return false;
    }

    private void ClearAllPointerState()
    {
        if (cachedPointerData == null || grid == null) return;

        foreach (var row in grid)
        {
            foreach (var sel in row.items)
            {
                if (sel != null)
                    ExecuteEvents.Execute(sel.gameObject, cachedPointerData, ExecuteEvents.pointerExitHandler);
            }
        }
    }

    private static bool DisableModuleAction(InputActionReference actionRef)
    {
        if (actionRef != null && actionRef.action != null && actionRef.action.enabled)
        {
            actionRef.action.Disable();
            return true;
        }
        return false;
    }

    private static void RestoreModuleAction(InputActionReference actionRef, bool wasEnabled)
    {
        if (wasEnabled && actionRef != null && actionRef.action != null)
            actionRef.action.Enable();
    }
}