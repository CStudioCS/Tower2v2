using UnityEngine;
using UnityEditor;

public class GrassTools : EditorWindow
{
    [MenuItem("Tower2v2/Grass/Randomize Grass #g")] // Hotkey: Shift + G
    public static void RandomizeGrass()
    {
        Grass[] allGrass = FindObjectsByType<Grass>(FindObjectsSortMode.None);

        Undo.RecordObjects(allGrass, "Randomize Grass");

        foreach (Grass grass in allGrass)
        {
            grass.RandomizeColor();
            grass.RandomizeOrientation();
            EditorUtility.SetDirty(grass);
        }

        Debug.Log($"Randomized {allGrass.Length} grass objects.");
    }
    
    [MenuItem("Tower2v2/Grass/Set Grass Debug Color #b")] // Hotkey: Shift + B
    public static void SetGrassDebugColor()
    {
        Grass[] allGrass = FindObjectsByType<Grass>(FindObjectsSortMode.None);

        Undo.RecordObjects(allGrass, "Set Grass Debug Color");

        foreach (Grass grass in allGrass)
        {
            grass.SetDebugColor();
            EditorUtility.SetDirty(grass);
        }

        Debug.Log($"Set Debug Color for {allGrass.Length} grass objects.");
    }
}