using UnityEngine;

public class MapSign: Interactable
{
	[SerializeField] private GameObject[] worldPrefabs;
	private int currentWorldIndex = 0;
	private GameObject CurrentWorldPrefab => worldPrefabs[currentWorldIndex];
	private int WorldCount => worldPrefabs.Length;
	[SerializeField] private GameObject firstMapInstance;
	private GameObject currentWorldInstance;

	protected override void Awake()
	{
		base.Awake();
		if (firstMapInstance == null)
			SpawnCurrentWorld();
		else
			currentWorldInstance = firstMapInstance;
	}

	public override float GetInteractionTime() => 0;

	public override bool CanInteract(Player player)
	{
		return true;
	}

	public override void Interact(Player player)
	{
		NextWorld();
	}

	private void NextWorld()
	{
		currentWorldIndex = (currentWorldIndex + 1) % WorldCount;
		if (currentWorldInstance != null)
		{
			Destroy(currentWorldInstance);
		}

		SpawnCurrentWorld();
	}

	private void SpawnCurrentWorld() => currentWorldInstance = Instantiate(CurrentWorldPrefab, Vector3.zero, Quaternion.identity);
}
