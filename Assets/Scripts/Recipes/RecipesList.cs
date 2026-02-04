using System.Collections.Generic;
using UnityEngine;

public class RecipesList : MonoBehaviour
{
    [SerializeField] private Recipe[] recipes;
    [SerializeField] private int queueSize = 5;

    private Dictionary<Item.Type, Recipe> recipesMap;
    private Dictionary<Item.Type, Recipe> RecipesMap
    {
        get
        {
            if (recipesMap != null) return recipesMap;
            recipesMap = new Dictionary<Item.Type, Recipe>();
            foreach (Recipe recipe in recipes)
            {
                recipesMap[recipe.Type] = recipe;
            }
            return recipesMap;
        }
    }

    [SerializeField] private Tower tower;
    private readonly Queue<Recipe> queue = new();
    private int randomIndex;
    
    private void OnEnable()
    {
        LevelManager.Instance.GameStarted += OnGameStarted;
        tower.PieceBuilt += OnPieceBuilt;
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
    
    public Item.Type CurrentNeededItemType => queue.Peek().Type;

    private void AddRecipe(Item.Type type)
    {
        if (!RecipesMap.TryGetValue(type, out Recipe recipe))
        {
            Debug.LogError($"Recipe with resource type {type} was not found.");
            return;
        }

        Recipe recipeInstance = Instantiate(recipe, transform);
        queue.Enqueue(recipeInstance);
    }

    private void AddRandomRecipe() => AddRecipe(ItemRandomizer.GetAt(randomIndex++));
    
    private void OnPieceBuilt() => CompleteRecipe();
    
    private void CompleteRecipe()
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
        tower.PieceBuilt -= OnPieceBuilt;
    }
}