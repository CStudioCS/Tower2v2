using Fusion;
using LitMotion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settings;
    [SerializeField] private EventSystem eventSystem;
    public EventSystem EventSystem => eventSystem;

    // --- 2D Input Handler ---
    [SerializeField] private MenuInputHandler inputHandler;
    public MenuInputHandler InputHandler => inputHandler;

    [Header("Navigation 2D")]
    [Tooltip("2D Grid of selectables for gamepad navigation.")]
    [SerializeField] private List<UIRow> menuRows = new List<UIRow>();

    // --- Slider Controller Tool ---
    [Tooltip("Component that handles left/right input dynamically if the target is a slider.")]
    [SerializeField] private SliderController sliderController;

    [Header("UI Elements")]
    [SerializeField] private GameObjectFadeIn creditsFader;
    [SerializeField] private GameObjectFadeIn networkFader;
    [SerializeField] private GameObjectFadeIn settingsFader;

    [Header("Network")]
    [SerializeField] private NetworkMenu networkMenu;
    private PlayerInput currentPlayer;
    private GameObject lastSelectedButton;

    public event Action Closed;

    public void ShowSettings(bool on = true) => settings.SetActive(on);

    // --- Event Subscriptions ---
    private void OnEnable()
    {
        // Delegate horizontal navigation: the controller will dynamically check if the selected target is a slider
        if (inputHandler != null && sliderController != null)
            inputHandler.OnCustomNavigation += sliderController.HandleNavigationInput;
    }

    private void OnDisable()
    {
        if (inputHandler != null && sliderController != null)
            inputHandler.OnCustomNavigation -= sliderController.HandleNavigationInput;
    }

    public void Open(PlayerInput playerInput)
    {
        currentPlayer = playerInput;
        if (!settingsFader.FadedIn) settingsFader.Fade();

        // Bind the generic input handler using our 2D Row Grid
        inputHandler.Bind(playerInput, eventSystem, Close, menuRows, lastSelectedButton);
    }

    public void GoToNetworkMenu()
    {
        lastSelectedButton = eventSystem.currentSelectedGameObject;

        if (settingsFader.FadedIn) settingsFader.Fade();
        networkMenu.Open(currentPlayer, this);
    }

    public void OnGoOnline(bool isOnline)
    {
        if (!isOnline)
            _ = NetworkManager.Instance.StartNetworkGame(GameMode.Single);
    }

    public void Close()
    {
        inputHandler.Unbind();
        eventSystem.SetSelectedGameObject(null);

        if (creditsFader.FadedIn) creditsFader.Fade();
        if (networkFader.FadedIn) networkFader.Fade();
        if (settingsFader.FadedIn) settingsFader.Fade();

        Closed?.Invoke();
    }
}