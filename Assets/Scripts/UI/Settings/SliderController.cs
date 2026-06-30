using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles horizontal input specifically for UI Sliders.
/// Designed to be decoupled from the main input handler.
/// </summary>
public class SliderController : MonoBehaviour
{
    [Tooltip("Step size for discrete slider movement with controller")]
    [SerializeField] private float sliderStepSize = 0.1f;

    /// <summary>
    /// Attempts to apply horizontal input to a slider. 
    /// Returns true if the input was consumed (i.e., a slider was modified).
    /// </summary>
    public bool HandleNavigationInput(GameObject target, Vector2 input)
    {
        if (target == null) return false;

        if (Mathf.Abs(input.x) <= Mathf.Abs(input.y)) return false;

        Slider activeSlider = target.GetComponent<Slider>() ?? target.GetComponentInParent<Slider>();

        if (activeSlider != null)
        {
            float direction = input.x > 0 ? 1f : -1f;
            activeSlider.value = Mathf.Clamp(activeSlider.value + direction * sliderStepSize, activeSlider.minValue, activeSlider.maxValue);

            return true;
        }

        return false;
    }
}