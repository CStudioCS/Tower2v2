using UnityEngine;

public class WorldPhysicsActivator: MonoBehaviour
{
	[SerializeField] private Collider2D[] colliders;
	[SerializeField] private Rigidbody2D[] rigidbodies;

	public Collider2D[] Colliders
	{
		get => colliders;
		set => colliders = value;
	}
	public Rigidbody2D[] Rigidbodies
	{
		get => rigidbodies;
		set => rigidbodies = value;
	}

	public void EnablePhysics(bool on = true)
	{
		foreach (Collider2D col in colliders)
			if (col != null)
				col.enabled = on;

		foreach (Rigidbody2D rb in rigidbodies)
			if (rb != null)
				rb.simulated = on;
	}

	public void DisablePhysics() => EnablePhysics(false);
}
