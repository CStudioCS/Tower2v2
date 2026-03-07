using UnityEngine;

public class StartPointLinker : MonoBehaviour
{
	[HideInInspector] public static StartPointLinker Instance;

	public StartPoint[] startPoints;

	private void Awake()
	{
		if(Instance != null)
			Destroy(Instance);

		Instance = this;
	}
}
