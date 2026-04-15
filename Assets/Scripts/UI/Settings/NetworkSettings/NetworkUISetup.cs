using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NetworkUISetup : MonoBehaviour
{
    [SerializeField] private UISwitcher switcher;
    [SerializeField] private Button configButton;
    [SerializeField] private TextMeshProUGUI configButtonText;

    private void Start()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.Runner != null)
        {
            bool isOnline = !NetworkManager.Instance.IsSinglePlayer;

            switcher.isOn = isOnline;
            OnStateChanged(isOnline);
        }

        switcher.onValueChanged.AddListener(OnStateChanged);
    }

    private void OnDestroy() => switcher.onValueChanged.RemoveListener(OnStateChanged);

    public void OnStateChanged(bool isOnline)
    {
        if (isOnline)
        {
            configButtonText.text = "Online";
            configButton.interactable = true;
        }
        else
        {
            configButtonText.text = "Offline";
            configButton.interactable = false;

            if (NetworkManager.Instance.IsHosting || NetworkManager.Instance.IsClient)
                NetworkManager.Instance.Reboot(GameMode.Single);
        }

    }
}