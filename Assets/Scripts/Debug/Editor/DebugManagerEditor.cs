#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(DebugManager))]
public class DebugManagerEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		DebugManager manager = (DebugManager)target;

		GUILayout.Space(10);
		GUILayout.Label("Hotkeys", EditorStyles.boldLabel);

		// Table headers
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Key", EditorStyles.boldLabel, GUILayout.Width(25));
		EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
		EditorGUILayout.EndHorizontal();

		MethodInfo[] methods = typeof(DebugManager).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		foreach (MethodInfo method in methods)
		{
			HotkeyAttribute attr = method.GetCustomAttribute<HotkeyAttribute>();
			if (attr != null)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(attr.Key.ToString(), EditorStyles.boldLabel, GUILayout.Width(25));
				EditorGUILayout.LabelField(attr.Description);
				EditorGUILayout.EndHorizontal();
			}
		}
	}
}
#endif
