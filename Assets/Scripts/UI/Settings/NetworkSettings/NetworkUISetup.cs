using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NetworkUISetup : MonoBehaviour
{
    [SerializeField] private Button configButton;
    [SerializeField] private TextMeshProUGUI configButtonText;

    [Header("Transition States")]
    [Tooltip("The colors of the button when the player is online")]
    [SerializeField] private ColorBlock onlineColors = ColorBlock.defaultColorBlock;

    private ColorBlock defaultOfflineColors;

    private void Awake()
        => defaultOfflineColors = configButton.colors;

    private void Start() => NetworkManager.OnNetworkModeInitialized += OnStateChanged;
    private void OnDestroy() => NetworkManager.OnNetworkModeInitialized -= OnStateChanged;

    public void OnStateChanged(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Single:
                configButtonText.text = "Offline";
                configButton.colors = defaultOfflineColors;
                break;

            case GameMode.Host:
                configButtonText.text = "Hosting";
                configButton.colors = onlineColors;
                break;

            case GameMode.Client:
                configButtonText.text = "Connected";
                configButton.colors = onlineColors;
                break;

            default:
                configButtonText.text = "Offline";
                configButton.colors = defaultOfflineColors;
                break;
        }
    }
}