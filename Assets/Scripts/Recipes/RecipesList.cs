using System.Collections.Generic;
using UnityEngine;

public class RecipesList : MonoBehaviour
{
    [SerializeField] private Recipe[] recipes;
    [SerializeField] private int queueSize = 5;

    private Dictionary<Resources.Type, Recipe> recipesMap;
    private Dictionary<Resources.Type, Recipe> RecipesMap
    {
        get
        {
            if (recipesMap == null)
            {
                recipesMap = new Dictionary<Resources.Type, Recipe>();
                foreach (Recipe recipe in recipes)
                {
                    recipesMap[recipe.Type] = recipe;
                }
            }
            return recipesMap;
        }
    }
    
    private Queue<Recipe> queue;

    private int randomIndex = 0;
    
    private void OnEnable()
    {
        LevelManager.Instance.GameStarted += OnGameStarted;
    }

    private void OnGameStarted()
    {
        ResourceRandomizer.TryReset();
        InitializeQueue();
    }

    private void InitializeQueue()
    {
        queue = new Queue<Recipe>();

        for (int i = 0; i < queueSize; i++)
        {
            AddRecipe(ResourceRandomizer.GetAt(randomIndex++));
        }
    }
    
    public Resources.Type CurrentNeededResourceType => queue.Peek().Type;

    private void AddRecipe(Resources.Type type)
    {
        if (!RecipesMap.TryGetValue(type, out Recipe recipe))
        {
            Debug.LogError($"Recipe with resource type {type} was not found.");
            return;
        }

        Recipe recipeInstance = Instantiate(recipe, transform);
        queue.Enqueue(recipeInstance);
    }

    private void AddRandomRecipe() => AddRecipe(ResourceRandomizer.GetAt(randomIndex++));
    
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