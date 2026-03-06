using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the master volume via AudioListener.volume.
/// Attach to the Sound GameObject that contains the Slider child.
/// Persists volume across sessions using PlayerPrefs.
/// </summary>
public class SoundSliderController : MonoBehaviour
{
    private const string VolumePrefKey = "MasterVolume";

    [SerializeField] private Slider slider;
    [SerializeField] private Image volumeIconImage;
    [SerializeField] private SoundVolumeIcon[] icons;

    /// <summary>
    /// Applies saved volume at game startup, before any scene loads.
    /// This runs even if the settings panel is inactive.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplySavedVolume()
    {
        AudioListener.volume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
    }

    private void Awake()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        slider.SetValueWithoutNotify(savedVolume);
        AudioListener.volume = savedVolume;

        slider.onValueChanged.AddListener(OnVolumeChanged);
        UpdateVolumeIcon(savedVolume);
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumePrefKey, value);
        UpdateVolumeIcon(value);
    }

    private void UpdateVolumeIcon(float value)
    {
        foreach (SoundVolumeIcon icon in icons)
        {
            if (value <= icon.maxVolume)
            {
                volumeIconImage.sprite = icon.sprite;
                return;
            }
        }
        // If value is greater than all maxVolumes, use the last icon
        volumeIconImage.sprite = icons[^1].sprite;
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}

[Serializable]
public class SoundVolumeIcon
{
    public Sprite sprite;
    public float maxVolume;
}
