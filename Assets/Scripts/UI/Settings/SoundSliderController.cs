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

    private void Awake()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        slider.SetValueWithoutNotify(savedVolume);
        AudioListener.volume = savedVolume;

        slider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumePrefKey, value);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}

