#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class InteractableIdScanner : EditorWindow
{
    [MenuItem("Tools/Network/Scan Interactable IDs")]
    public static void ScanAllIds()
    {
        HashSet<string> usedIds = new HashSet<string>();
        int fixedCount = 0;

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Interactable[] interactables = prefab.GetComponentsInChildren<Interactable>(true);
            bool prefabModified = false;

            foreach (var interactable in interactables)
            {
                SerializedObject so = new SerializedObject(interactable);
                SerializedProperty idProp = so.FindProperty("uniqueId");

                if (string.IsNullOrEmpty(idProp.stringValue) || usedIds.Contains(idProp.stringValue))
                {
                    idProp.stringValue = System.Guid.NewGuid().ToString();
                    so.ApplyModifiedProperties(); 
                    prefabModified = true;
                    fixedCount++;
                }

                usedIds.Add(idProp.stringValue);
            }

            if (prefabModified)
            {
                EditorUtility.SetDirty(prefab);
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }


        Interactable[] sceneInteractables = Object.FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var interactable in sceneInteractables)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(interactable)) continue;

            SerializedObject so = new SerializedObject(interactable);
            SerializedProperty idProp = so.FindProperty("uniqueId");

            if (string.IsNullOrEmpty(idProp.stringValue) || usedIds.Contains(idProp.stringValue))
            {
                idProp.stringValue = System.Guid.NewGuid().ToString();
                so.ApplyModifiedProperties();

                PrefabUtility.RecordPrefabInstancePropertyModifications(interactable);
                fixedCount++;
            }

            usedIds.Add(idProp.stringValue);
        }

        Debug.Log($"Scan finished! {fixedCount} missing or duplicate IDs have been fixed and saved.");
    }
}

#endif