using System;
using UnityEngine;
using LitMotion;

public class MapSign: Sign
{
	[SerializeField] private WorldPhysicsActivator[] worldPrefabs;
	private int currentWorldIndex = 0;
	private WorldPhysicsActivator CurrentWorldPrefab => worldPrefabs[currentWorldIndex];
	private int WorldCount => worldPrefabs.Length;
	[SerializeField] private WorldPhysicsActivator firstMapInstance;
	private WorldPhysicsActivator currentWorldInstance;
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
			WorldPhysicsActivator oldWorld = currentWorldInstance;
			oldWorld.DisablePhysics();
			TransitionOldWorld(oldWorld);
		}

		SpawnCurrentWorld(Vector2.right * worldWidth);
		if (currentWorldInstance == null)
		{
			transitioning = false;
			return;
		}
		currentWorldInstance.DisablePhysics();
		TransitionCurrentWorld();
	}

	private void SpawnCurrentWorld(Vector2 position)
	{
		if (CurrentWorldPrefab == null)
		{
			Debug.LogError($"[MapSign] worldPrefabs[{currentWorldIndex}] is null! Make sure all slots are assigned in the inspector.");
			return;
		}
		currentWorldInstance = Instantiate(CurrentWorldPrefab, position, Quaternion.identity);
	}

	private void TransitionOldWorld(WorldPhysicsActivator oldWorld) => TransitionWorld(oldWorld, Vector2.left * worldWidth, () => Destroy(oldWorld.gameObject));
	private void TransitionCurrentWorld() => TransitionWorld(currentWorldInstance, Vector2.zero, () =>
	{
		transitioning = false;
		currentWorldInstance.EnablePhysics();
	});

	private void TransitionWorld(WorldPhysicsActivator world, Vector2 targetPosition, Action onComplete = null)
	{
		LMotion.Create((Vector2) world.transform.position, targetPosition, transitionDuration)
			   .WithEase(Ease.OutQuad)
			   .WithOnComplete(onComplete)
			   .Bind(pos => world.transform.position = pos)
			   .AddTo(world);
	}
}
