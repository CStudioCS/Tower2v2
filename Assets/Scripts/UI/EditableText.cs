using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EditableText : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private TMP_InputField inputField;

    [Header("Gamepad Settings")]
    [Tooltip("Maximum time in seconds between presses to count as a double-click.")]
    [SerializeField] private float gamepadDoubleClickThreshold = 0.4f;

    [Header("Default Settings")]
    [SerializeField] private bool isEditable = true;

    public event Action<string> OnValueChanged;

    // --- Dynamic Dependencies ---
    private MenuInputHandler activeMenuHandler;
    private float lastSubmitTime = 0f;
    private bool wasModuleEnabledBeforeEdit;
    private string defaultText;

    private void Awake()
    {
        inputField.onEndEdit.AddListener(OnFinishEditing);
        SetEditingState(false);
    }

    /// <summary>
    /// Called by the LobbyRowController when the prefab is instantiated.
    /// </summary>
    public void Initialize(MenuInputHandler menuHandler, string initialText, bool editable = true)
    {
        isEditable = editable;
        activeMenuHandler = menuHandler;
        defaultText = initialText;
        displayText.text = initialText;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
    }

    public TextMeshProUGUI DisplayText { get => displayText; set => displayText = value; }

    // --- MOUSE DOUBLE CLICK ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEditable) return;

        if (eventData.clickCount == 2)
            StartEditing();
    }

    // --- GAMEPAD DOUBLE CLICK ---
    public void OnSubmit(BaseEventData eventData)
    {
        if (!isEditable) return;

        if (Time.unscaledTime - lastSubmitTime < gamepadDoubleClickThreshold)
            StartEditing();

        lastSubmitTime = Time.unscaledTime;
    }

    private void StartEditing()
    {
        activeMenuHandler.enabled = false;

        if (activeMenuHandler.UIModule != null)
        {
            wasModuleEnabledBeforeEdit = activeMenuHandler.UIModule.enabled;
            activeMenuHandler.UIModule.enabled = true;
        }

        inputField.text = displayText.text;
        SetEditingState(true);

        inputField.Select();
        inputField.ActivateInputField();
    }

    private void OnFinishEditing(string newText)
    {
        if (string.IsNullOrWhiteSpace(newText))
            newText = defaultText;

        displayText.text = newText;
        SetEditingState(false);

        OnValueChanged?.Invoke(newText);

        if (activeMenuHandler.UIModule != null)
            activeMenuHandler.UIModule.enabled = wasModuleEnabledBeforeEdit;

        activeMenuHandler.enabled = true;
    }

    private void SetEditingState(bool isEditing)
    {
        displayText.gameObject.SetActive(!isEditing);
        inputField.gameObject.SetActive(isEditing);

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform.parent);
    }

    private void OnDestroy()
    {
        inputField.onEndEdit.RemoveListener(OnFinishEditing);
    }
}