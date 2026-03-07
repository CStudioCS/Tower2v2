using System;
using UnityEngine;
using LitMotion;

public class MapSign: Sign
{
	[SerializeField] private GameObject[] worldPrefabs;
	private int currentWorldIndex = 0;
	private GameObject CurrentWorldPrefab => worldPrefabs[currentWorldIndex];
	private int WorldCount => worldPrefabs.Length;
	[SerializeField] private GameObject firstMapInstance;
	private GameObject currentWorldInstance;
	[SerializeField] private float worldWidth = 20;
	[SerializeField] private float transitionDuration = 1f;
	private bool transitioning;

	protected override void Awake()
	{
		base.Awake();
		if (firstMapInstance == null)
			SpawnCurrentWorld(Vector2.zero);
		else
			currentWorldInstance = firstMapInstance;
	}

	public override void Interact(Player player)
	{
		if (transitioning)
			return;
		
		NextWorld();
	}

	private void NextWorld()
	{
		transitioning = true;
		currentWorldIndex = (currentWorldIndex + 1) % WorldCount;
		if (currentWorldInstance != null)
		{
			GameObject oldWorld = currentWorldInstance;
			DisableWorld(oldWorld);
			TransitionOldWorld(oldWorld);
		}

		SpawnCurrentWorld(Vector2.right * worldWidth);
		TransitionCurrentWorld();
	}

	private void SpawnCurrentWorld(Vector2 position) => currentWorldInstance = Instantiate(CurrentWorldPrefab, position, Quaternion.identity);

	private void TransitionOldWorld(GameObject oldWorld) => TransitionWorld(oldWorld, Vector2.left * worldWidth, () => Destroy(oldWorld));
	private void TransitionCurrentWorld() => TransitionWorld(currentWorldInstance, Vector2.zero, () => transitioning = false);

	private void TransitionWorld(GameObject world, Vector2 targetPosition, Action onComplete = null)
	{
		LMotion.Create((Vector2) world.transform.position, targetPosition, transitionDuration)
			   .WithEase(Ease.OutQuad)
			   .WithOnComplete(onComplete)
			   .Bind(pos => world.transform.position = pos)
			   .AddTo(world);
	}
	
	// This is not super clean, this should probably be cached to avoid getting the components at runtime. TODO fix
	public void DisableWorld(GameObject world)
	{
		foreach (Collider2D col in world.GetComponentsInChildren<Collider2D>())
			col.enabled = false;

		foreach (Rigidbody2D rb in world.GetComponentsInChildren<Rigidbody2D>())
			rb.simulated = false;

		foreach (MonoBehaviour behaviour in world.GetComponentsInChildren<MonoBehaviour>())
			behaviour.enabled = false;
	}
}
