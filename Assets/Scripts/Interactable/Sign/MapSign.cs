using System;
using UnityEngine;
using LitMotion;

public class MapSign: Sign
{
	[SerializeField] private WorldPhysicsActivator[] worldPrefabs;
	private int WorldCount => worldPrefabs.Length;
	[SerializeField] private WorldPhysicsActivator firstMapInstance;
	private WorldPhysicsActivator currentWorldInstance;
	[SerializeField] private float worldWidth = 20;
	[SerializeField] private float transitionDuration = 1f;
	private bool transitioning;
    private int currentWorldIndex = 0;

    protected override void Awake()
	{
		base.Awake();
		if (firstMapInstance == null)
			SpawnCurrentWorld(Vector2.zero, 0);
		else
			currentWorldInstance = firstMapInstance;
	}

    private void OnEnable() => LobbyManager.OnMapChangedEvent += HandleMapChanged;
    private void OnDisable() => LobbyManager.OnMapChangedEvent -= HandleMapChanged;

    public override void Interact(Player player)
	{
		if (transitioning)
			return;

        if (LobbyManager.Instance != null && LobbyManager.Instance.HasStateAuthority)
            LobbyManager.Instance.CurrentMapIndex = (LobbyManager.Instance.CurrentMapIndex + 1) % WorldCount;
	}

    private void HandleMapChanged(int newMapIndex, bool animate)
    {
        if (currentWorldIndex == newMapIndex) return;

        currentWorldIndex = newMapIndex;

        if (animate)
            TransitionToNewWorld(newMapIndex);
        else
            InstantSnapToNewWorld(newMapIndex); // For late joiners
    }

    private void TransitionToNewWorld(int newIndex)
    {
        transitioning = true;

        if (currentWorldInstance != null)
        {
            WorldPhysicsActivator oldWorld = currentWorldInstance;
            oldWorld.DisablePhysics();
            TransitionOldWorld(oldWorld);
        }

        SpawnCurrentWorld(Vector2.right * worldWidth, newIndex);

        if (currentWorldInstance == null)
        {
            transitioning = false;
            return;
        }

        currentWorldInstance.DisablePhysics();
        TransitionCurrentWorld();
    }

    private void InstantSnapToNewWorld(int newIndex)
    {
        if (currentWorldInstance != null)
            Destroy(currentWorldInstance.gameObject);

        SpawnCurrentWorld(Vector2.zero, newIndex);
    }

    private void SpawnCurrentWorld(Vector2 position, int index)
	{
		if (worldPrefabs[index] == null)
		{
			Debug.LogError($"[MapSign] worldPrefabs[{index}] is null! Make sure all slots are assigned in the inspector.");
			return;
		}
        currentWorldInstance = Instantiate(worldPrefabs[index], position, Quaternion.identity);
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
