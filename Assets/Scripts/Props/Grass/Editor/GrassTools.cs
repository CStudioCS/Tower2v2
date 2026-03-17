using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class GrassTools: EditorWindow
{
	// ---------------- PREFAB-MODE / SELECTED PREFAB METHODS ----------------

    [MenuItem("Tower2v2/Grass/Prefab/Randomize Grass #g")]
    public static void RandomizeCurrentPrefabGrass()
    {
        GameObject prefabRoot = GetCurrentPrefabRoot();
        if (prefabRoot == null)
        {
            Debug.LogWarning("No prefab currently being edited or selected.");
            return;
        }

        Grass[] grasses = prefabRoot.GetComponentsInChildren<Grass>(true);
        foreach (Grass grass in grasses)
        {
            if (grass.Color == Grass.DebugColor)
			{
				grass.Randomize();
                EditorUtility.SetDirty(grass);
            }
        }

        Debug.Log($"Randomized {grasses.Length} grass objects in the current prefab.");
    }

    [MenuItem("Tower2v2/Grass/Prefab/Re-randomize All Grass &#g")]
    public static void RandomizeAllCurrentPrefabGrass()
    {
        GameObject prefabRoot = GetCurrentPrefabRoot();
        if (prefabRoot == null)
        {
            Debug.LogWarning("No prefab currently being edited or selected.");
            return;
        }

        Grass[] grasses = prefabRoot.GetComponentsInChildren<Grass>(true);
        foreach (Grass grass in grasses)
        {
			grass.Randomize();
            EditorUtility.SetDirty(grass);
        }

        Debug.Log($"Randomized ALL {grasses.Length} grass objects in the current prefab.");
    }

    [MenuItem("Tower2v2/Grass/Prefab/Set Grass Debug Color %#b")]
    public static void SetDebugColorCurrentPrefabGrass()
    {
        GameObject prefabRoot = GetCurrentPrefabRoot();
        if (prefabRoot == null)
        {
            Debug.LogWarning("No prefab currently being edited or selected.");
            return;
        }

        Grass[] grasses = prefabRoot.GetComponentsInChildren<Grass>(true);
        foreach (Grass grass in grasses)
        {
            grass.SetDebugColor();
            EditorUtility.SetDirty(grass);
        }

        Debug.Log($"Set debug color for {grasses.Length} grass objects in the current prefab.");
    }
	
	// ---------------- SCENE METHODS ----------------

	[MenuItem("Tower2v2/Grass/Scene/Randomize Grass %#g")]
	public static void RandomizeSceneGrass()
	{
		Grass[] sceneGrass = Object.FindObjectsByType<Grass>(FindObjectsSortMode.None);
		Undo.RecordObjects(sceneGrass, "Randomize Scene Grass");

		foreach (Grass grass in sceneGrass)
		{
			if (grass.Color == Grass.DebugColor)
			{
				grass.Randomize();
				EditorUtility.SetDirty(grass);
			}
		}

		Debug.Log($"Randomized {sceneGrass.Length} scene grass objects.");
	}

	[MenuItem("Tower2v2/Grass/Scene/Re-randomize All Grass %&#g")]
	public static void RandomizeAllSceneGrass()
	{
		Grass[] sceneGrass = Object.FindObjectsByType<Grass>(FindObjectsSortMode.None);
		Undo.RecordObjects(sceneGrass, "Randomize All Scene Grass");

		foreach (Grass grass in sceneGrass)
		{
			grass.Randomize();
			EditorUtility.SetDirty(grass);
		}

		Debug.Log($"Randomized ALL {sceneGrass.Length} scene grass objects.");
	}

	[MenuItem("Tower2v2/Grass/Scene/Set Grass Debug Color #b")]
	public static void SetSceneGrassDebugColor()
	{
		Grass[] sceneGrass = FindObjectsByType<Grass>(FindObjectsSortMode.None);
		Undo.RecordObjects(sceneGrass, "Set Scene Grass Debug Color");

		foreach (Grass grass in sceneGrass)
		{
			grass.SetDebugColor();
			EditorUtility.SetDirty(grass);
		}

		Debug.Log($"Set Debug Color for {sceneGrass.Length} scene grass objects.");
	}

	// ---------------- HELPER ----------------

	private static GameObject GetCurrentPrefabRoot()
	{
		GameObject stageGO = PrefabStageUtility.GetCurrentPrefabStage()?.prefabContentsRoot;
		if (stageGO != null) return stageGO;

		GameObject selected = Selection.activeObject as GameObject;
		if (selected != null)
		{
			string path = AssetDatabase.GetAssetPath(selected);
			if (PrefabUtility.IsPartOfPrefabAsset(selected))
			{
				return PrefabUtility.LoadPrefabContents(path);
			}
		}

		return null;
	}
}
