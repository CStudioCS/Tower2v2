using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class WorldPhysicsActivatorPrefabProcessor
{
	static WorldPhysicsActivatorPrefabProcessor()
	{
		PrefabStage.prefabSaving += OnPrefabSaving;
	}

	private static void OnPrefabSaving(GameObject prefabRoot)
	{
		foreach (WorldPhysicsActivator activator in prefabRoot.GetComponentsInChildren<WorldPhysicsActivator>(true))
		{
			activator.Colliders = activator.GetComponentsInChildren<Collider2D>(true);
			activator.Rigidbodies = activator.GetComponentsInChildren<Rigidbody2D>(true);

			EditorUtility.SetDirty(activator);

			Debug.Log($"[Prefab Save] Cached {activator.Colliders.Length} colliders and {activator.Rigidbodies.Length} rigidbodies in {activator.name}");
		}
	}
}
