#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UISwitcher))]
public class UISwitcherEditor : UnityEditor.UI.SelectableEditor
{
    private UISwitcher _switcher;
    private SerializedProperty _isOn, _onValueChangedEvent;

    // Visuals
    private SerializedProperty _tipRect;
    private SerializedProperty _backgroundGraphic;

    // Animation
    private SerializedProperty _animationDuration;

    // Colors
    private SerializedProperty _onColor;
    private SerializedProperty _onHighlightedColor;
    private SerializedProperty _offColor;
    private SerializedProperty _offHighlightedColor;
    private SerializedProperty _disabledColor;

    private const string MIXED_FIELD_NAME = "Is On";

    protected override void OnEnable()
    {
        base.OnEnable();
        _switcher = serializedObject.targetObject as UISwitcher;

        _isOn = serializedObject.FindProperty("m_isOn");
        _onValueChangedEvent = serializedObject.FindProperty("onValueChanged");

        _tipRect = serializedObject.FindProperty("tipRect");
        _backgroundGraphic = serializedObject.FindProperty("backgroundGraphic");

        _animationDuration = serializedObject.FindProperty("animationDuration");

        _onColor = serializedObject.FindProperty("onColor");
        _onHighlightedColor = serializedObject.FindProperty("onHighlightedColor");

        _offColor = serializedObject.FindProperty("offColor");
        _offHighlightedColor = serializedObject.FindProperty("offHighlightedColor");

        _disabledColor = serializedObject.FindProperty("disabledColor");
    }

    public override void OnInspectorGUI()
    {
        // Always call Update() at the beginning of a Custom Editor
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        // Main toggle
        EditorGUILayout.PropertyField(_isOn, new GUIContent(MIXED_FIELD_NAME));

        EditorGUILayout.Space();

        // Visuals
        EditorGUILayout.LabelField("Visuals", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_backgroundGraphic);
        EditorGUILayout.PropertyField(_tipRect);

        EditorGUILayout.Space();

        // Animation
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_animationDuration);

        EditorGUILayout.Space();

        // ON Colors
        EditorGUILayout.LabelField("ON State Colors", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_onColor);
        EditorGUILayout.PropertyField(_onHighlightedColor);

        EditorGUILayout.Space();

        // OFF Colors
        EditorGUILayout.LabelField("OFF State Colors", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_offColor);
        EditorGUILayout.PropertyField(_offHighlightedColor);

        EditorGUILayout.Space();

        // Disabled Color
        EditorGUILayout.LabelField("Disabled State Color", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_disabledColor);

        EditorGUILayout.Space();

        // Events
        EditorGUILayout.PropertyField(_onValueChangedEvent, true);

        // Only force visual update in the scene if a value was actually modified
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            _switcher.OnChanged();
            EditorUtility.SetDirty(_switcher);
        }
        else
        {
            serializedObject.ApplyModifiedProperties();
        }

        DrawUILine(Color.black);

        // Draw the native Selectable part (Interactable, Navigation...)
        base.OnInspectorGUI();
    }

    private static void DrawUILine(Color color, int thickness = 1, int padding = 10)
    {
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        rect.height = thickness;
        rect.y += padding / 2;
        rect.x -= 2;
        rect.width += 6;
        EditorGUI.DrawRect(rect, color);
    }
}
#endif