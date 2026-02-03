using System.Collections.Generic;
using UnityEngine;

public class RecipesList : MonoBehaviour
{
    [SerializeField] private Recipe[] recipes;
    [SerializeField] private int queueSize = 5;

    private Dictionary<WorldResources.Type, Recipe> recipesMap;
    private Dictionary<WorldResources.Type, Recipe> RecipesMap
    {
        get
        {
            if (recipesMap == null)
            {
                recipesMap = new Dictionary<WorldResources.Type, Recipe>();
                foreach (Recipe recipe in recipes)
                {
                    recipesMap[recipe.Type] = recipe;
                }
            }
            return recipesMap;
        }
    }
    
    private readonly Queue<Recipe> queue = new();

    private int randomIndex = 0;
    
    private void OnEnable()
    {
        LevelManager.Instance.GameStarted += OnGameStarted;
    }

    private void OnGameStarted()
    {
        randomIndex = 0;
        InitializeQueue();
    }
    
    private void ClearQueue()
    {
        while (queue.Count > 0)
        {
            Recipe recipe = queue.Dequeue();
            Destroy(recipe.gameObject);
        }

        queue.Clear();
    }

    private void InitializeQueue()
    {
        ClearQueue();

        for (int i = 0; i < queueSize; i++)
        {
            AddRandomRecipe();
        }
    }
    
    public WorldResources.Type CurrentNeededResourceType => queue.Peek().Type;

    private void AddRecipe(WorldResources.Type type)
    {
        if (!RecipesMap.TryGetValue(type, out Recipe recipe))
        {
            Debug.LogError($"Recipe with resource type {type} was not found.");
            return;
        }

        Recipe recipeInstance = Instantiate(recipe, transform);
        queue.Enqueue(recipeInstance);
    }

    private void AddRandomRecipe() => AddRecipe(WorldResourceRandomizer.GetAt(randomIndex++));
    
    public void OnRecipeCompleted()
    {
        if (queue.Count > 0)
        {
            var recipe = queue.Dequeue();
            Destroy(recipe.gameObject);
            AddRandomRecipe();
        }
    }
    
    private void OnDestroy()
    {
        LevelManager.Instance.GameStarted -= OnGameStarted;
    }
}