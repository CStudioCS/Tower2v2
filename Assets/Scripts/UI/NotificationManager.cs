using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private GameObjectFadeIn fader;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;

    private Coroutine activeSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// The function to call from anywhere to show a popup message.
    /// </summary>
    public void ShowNotification(string message)
    {
        if (activeSequence != null)
            StopCoroutine(activeSequence);

        notificationText.text = message;
        activeSequence = StartCoroutine(NotificationSequence());
    }

    private IEnumerator NotificationSequence()
    {
        notificationPanel.SetActive(true);

        if (!fader.FadedIn)
            fader.Fade();

        yield return new WaitForSecondsRealtime(displayDuration);

        if (fader.FadedIn)
            fader.Fade();

        yield return new WaitForSecondsRealtime(fader.Duration);

        notificationPanel.SetActive(false);
        activeSequence = null;
    }
}