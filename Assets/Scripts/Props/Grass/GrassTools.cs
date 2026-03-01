using UnityEngine;
using UnityEditor;

public class GrassTools : EditorWindow
{
    [MenuItem("Tower2v2/Grass/Randomize All Grass Colors #g")] // Hotkey: Shift + G
    public static void RandomizeAllGrassColors()
    {
        Grass[] allGrass = FindObjectsByType<Grass>(FindObjectsSortMode.None);

        Undo.RecordObjects(allGrass, "Randomize Grass Colors");

        foreach (Grass grass in allGrass)
        {
            grass.ApplyRandomColor();
            EditorUtility.SetDirty(grass);
        }

        Debug.Log($"Randomized colors for {allGrass.Length} grass objects.");
    }
}