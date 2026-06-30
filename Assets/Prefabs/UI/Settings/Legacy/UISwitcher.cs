using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[Serializable]
public class ValueChangedEvent : UnityEvent<bool> { }

public class UISwitcher : Selectable, IPointerClickHandler, ISubmitHandler
{
    public event Action<bool> OnValueChanged;
    public ValueChangedEvent onValueChanged = new();

    [SerializeField] private bool m_isOn;

    public bool isOn
    {
        get => m_isOn;
        set => Set(value);
    }

    private readonly Vector2 _min = new(0.6f, 0.5f);
    private readonly Vector2 _max = new(1, 0.5f);


    [SerializeField] private Graphic backgroundGraphic;
    [SerializeField] private RectTransform tipRect;


    [SerializeField] private float animationDuration = 0.15f;
    private Coroutine moveCoroutine;

    [SerializeField] private Color onColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color onHighlightedColor = new Color(0.3f, 0.9f, 0.3f);

    [SerializeField] private Color offColor = new Color(0.5f, 0.5f, 0.5f);
    [SerializeField] private Color offHighlightedColor = new Color(0.6f, 0.6f, 0.6f);

    [SerializeField] private Color disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private Color backgroundColor
    {
        get => backgroundGraphic != null ? backgroundGraphic.color : Color.white;
        set { if (backgroundGraphic != null) backgroundGraphic.color = value; }
    }

    protected override void Start()
    {
        base.Start();
        UpdateVisuals(true);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        UpdateVisuals(false);
    }

    public void SetWithoutNotify(bool value) => Set(value, false);

    private void Set(bool value, bool notify = true)
    {
        if (m_isOn == value) return;

        m_isOn = value;

        if (notify) ValueChangedNotify(value);
        OnChanged(value);
    }

    public void OnChanged() => OnChanged(m_isOn);

    protected void OnChanged(bool value)
    {
        UpdateVisuals(false);
    }

    private void ValueChangedNotify(bool value)
    {
        OnValueChanged?.Invoke(value);
        onValueChanged?.Invoke(value);
    }

    public void OnPointerClick(PointerEventData eventData) => MoveToBetweenTrueFalse();
    public void OnSubmit(BaseEventData eventData) => MoveToBetweenTrueFalse();

    private void MoveToBetweenTrueFalse()
    {
        if (!IsActive() || !IsInteractable()) return;
        isOn = !isOn;
    }

    private void UpdateVisuals(bool instant)
    {
        bool isInteractable = IsInteractable();

        bool isHighlighted = isInteractable &&
                            (currentSelectionState == SelectionState.Highlighted ||
                             currentSelectionState == SelectionState.Selected ||
                             currentSelectionState == SelectionState.Pressed);

        Vector2 targetAnchor = m_isOn ? _max : _min;
        Color targetColor;

        // Choix de la couleur finale
        if (!isInteractable)
        {
            targetColor = disabledColor;
        }
        else if (m_isOn)
        {
            targetColor = isHighlighted ? onHighlightedColor : onColor;
        }
        else
        {
            targetColor = isHighlighted ? offHighlightedColor : offColor;
        }

        if (instant || !Application.isPlaying)
        {
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            SetAnchors(targetAnchor);
            backgroundColor = targetColor;
        }
        else
        {
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(AnimateVisuals(targetAnchor, targetColor));
        }
    }

    private IEnumerator AnimateVisuals(Vector2 targetAnchor, Color targetColor)
    {
        if (tipRect == null) yield break;

        Vector2 initialAnchor = tipRect.anchorMin;
        Color initialColor = backgroundColor;
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / animationDuration;

            SetAnchors(Vector2.Lerp(initialAnchor, targetAnchor, t));
            backgroundColor = Color.Lerp(initialColor, targetColor, t);

            yield return null;
        }

        SetAnchors(targetAnchor);
        backgroundColor = targetColor;
        moveCoroutine = null;
    }

    private void SetAnchors(Vector2 anchor)
    {
        if (tipRect == null) return;
        tipRect.anchorMin = anchor;
        tipRect.anchorMax = anchor;
        tipRect.pivot = anchor;
    }
}