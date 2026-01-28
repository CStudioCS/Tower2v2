using System.Collections.Generic;
using UnityEngine;

public class RecipesList : MonoBehaviour
{
    [SerializeField] private Recipe[] recipes;
    [SerializeField] private int queueSize = 5;

    private Dictionary<Resources.Type, Recipe> recipesMap = new();
    private Queue<Recipe> queue;

    private int randomIndex = 0;
    
    private void Start()
    {
        InitializeRecipesMap();
        InitializeQueue();
    }

    private void InitializeRecipesMap()
    {
        foreach (Recipe recipe in recipes)
        {
            recipesMap[recipe.Type] = recipe;
        }
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
        if (!recipesMap.TryGetValue(type, out Recipe recipe))
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
}