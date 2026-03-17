using System.Collections.Generic;
using UnityEngine;

public class RecipeBannerLinker : MonoBehaviour
{
	public static RecipeBannerLinker Instance { get; private set; }

	[SerializeField] private RecipesList recipeBannerLeft;
	public RecipesList RecipeBannerLeft => recipeBannerLeft;
	[SerializeField] private RecipesList recipeBannerRight;
	public RecipesList RecipeBannerRight => recipeBannerRight;

	private void Awake()
	{
		// This is not a true singleton, as the world may change and the towers will change.
		// It is true that at a given point in time, there should be only one active Instance.
		// But any new Instance overrides the previous one.
		Instance = this;
	}
	
	private Dictionary<PlayerTeam.Team, RecipesList> recipeBannerMap;
	public Dictionary<PlayerTeam.Team, RecipesList> RecipeBannerMap
	{
		get
		{
			recipeBannerMap ??= new Dictionary<PlayerTeam.Team, RecipesList>
			{
				[PlayerTeam.Team.Left] = recipeBannerLeft,
				[PlayerTeam.Team.Right] = recipeBannerRight
			};
			return recipeBannerMap;
		}
	}
}
