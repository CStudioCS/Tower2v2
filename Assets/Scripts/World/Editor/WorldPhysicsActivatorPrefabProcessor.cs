using System.Collections.Generic;
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
			Collider2D[] allColliders = activator.GetComponentsInChildren<Collider2D>(true);
			List<Collider2D> enabledColliders = new List<Collider2D>();
			foreach (Collider2D c in allColliders)
			{
				if (c.enabled)
					enabledColliders.Add(c);
			}

			activator.Colliders = enabledColliders.ToArray();

			Rigidbody2D[] allRigidbodies = activator.GetComponentsInChildren<Rigidbody2D>(true);
			List<Rigidbody2D> simulatedRigidbodies = new List<Rigidbody2D>();
			foreach (Rigidbody2D rb in allRigidbodies)
			{
				if (rb.simulated)
					simulatedRigidbodies.Add(rb);
			}

			activator.Rigidbodies = simulatedRigidbodies.ToArray();

			EditorUtility.SetDirty(activator);

			Debug.Log($"[Prefab Save] Cached {activator.Colliders.Length} enabled colliders and {activator.Rigidbodies.Length} simulated rigidbodies in {activator.name}");
		}
	}
}
