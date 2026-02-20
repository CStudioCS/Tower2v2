using System.Collections.Generic;
using UnityEngine;

public class RecipesList : MonoBehaviour
{
    [SerializeField] private Recipe[] recipePrefabs;
    [SerializeField] private RecipeSlot[] recipeSlots;
    
    private Dictionary<Item.Type, Recipe> recipesMap;
    private Dictionary<Item.Type, Recipe> RecipesMap
    {
        get
        {
            if (recipesMap != null) return recipesMap;
            recipesMap = new Dictionary<Item.Type, Recipe>();
            foreach (Recipe recipe in recipePrefabs)
            {
                recipesMap[recipe.Type] = recipe;
            }
            return recipesMap;
        }
    }

    private bool IsLeftRecipeList => this == CanvasLinker.Instance.recipesListLeft; 
    private Tower Tower => IsLeftRecipeList ? WorldLinker.Instance.towerLeft : WorldLinker.Instance.towerRight;

    // The recipes array is cyclic. The index of the first recipe is dynamic and stored in firstRecipeIndex.
    // This avoids the need to shift the array when a recipe is completed.
    private Recipe[] recipes;
    private int firstRecipeIndex = 0;
    private int randomIndex;

    private void Start()
    {
        Tower.PieceBuilt += OnPieceBuilt;
        randomIndex = 0;
        InitializeRecipes();
    }

    private void ClearRecipes()
    {
        foreach (Recipe recipe in recipes)
        {
            Destroy(recipe.gameObject);
        }
    }

    private void InitializeRecipes()
    {
        if (recipes == null)
            recipes = new Recipe[recipeSlots.Length];
        else
            ClearRecipes();
        
        for (int i = 0; i < recipeSlots.Length; i++)
        {
            AddRandomRecipe(i, i);
        }
    }
    
    public Item.Type CurrentNeededItemType => recipes[firstRecipeIndex].Type;

    private void AddRecipe(Item.Type type, int recipesIndex, int slotIndex)
    {
        if (!RecipesMap.TryGetValue(type, out Recipe recipe))
        {
            Debug.LogError($"Recipe with resource type {type} was not found.");
            return;
        }

        Recipe recipeInstance = Instantiate(recipe, transform);
        recipes[recipesIndex] = recipeInstance;
        recipeInstance.SetRecipeSlot(recipeSlots[slotIndex % recipeSlots.Length]);
    }
    
    private void AddRandomRecipe(int recipesIndex, int slotIndex) => AddRecipe(ItemRandomizer.Instance.GetAt(randomIndex++), recipesIndex, slotIndex);
    
    private void OnPieceBuilt() => CompleteRecipe();
    
    private void CompleteRecipe()
    {
        Destroy(recipes[firstRecipeIndex].gameObject);
        
        for (int i = 1; i < recipeSlots.Length; i++)
        {
            int cyclicIndex = (firstRecipeIndex + i) % recipeSlots.Length;
            recipes[cyclicIndex].SetRecipeSlot(recipeSlots[i - 1]);
        }
        
        AddRandomRecipe(firstRecipeIndex, recipeSlots.Length - 1);
        firstRecipeIndex = (firstRecipeIndex + 1) % recipeSlots.Length;
    }
    
    private void OnDisable()
    {
        Tower.PieceBuilt -= OnPieceBuilt;
    }
}