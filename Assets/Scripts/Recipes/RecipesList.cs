using System;
using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
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
    private int firstRecipeIndex;
    private int randomIndex;
    
    private Recipe CurrentRecipe => recipes[firstRecipeIndex];
    public Item.Type CurrentNeededItemType => CurrentRecipe.Type;
    
    [SerializeField] private UnityEngine.UI.Graphic mainPanelToColorize;
    [SerializeField] private Color validateColor = Color.green;
    [SerializeField] private Color invalidateColor = Color.red;
    [SerializeField] private float colorFlashDuration = 0.1f;
    [SerializeField] private float colorStayDuration = 0.2f;

    private MotionHandle colorTweenHandle;

    private void Start()
    {
        Tower.PieceBuilt += OnPieceBuilt;
        Tower.TriedBuildingWithIncorrectItemType += OnTriedBuildingWithIncorrectItemType;
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
    
    private void AddRecipe(Item.Type type, int recipesIndex, int slotIndex, bool animate = false)
    {
        if (!RecipesMap.TryGetValue(type, out Recipe recipe))
        {
            Debug.LogError($"Recipe with resource type {type} was not found.");
            return;
        }

        Recipe recipeInstance = Instantiate(recipe, transform);
        recipes[recipesIndex] = recipeInstance;
        recipeInstance.Appear(recipeSlots[slotIndex % recipeSlots.Length], animate);
    }
    
    private void AddRandomRecipe(int recipesIndex, int slotIndex, bool animate = false) => AddRecipe(ItemRandomizer.Instance.GetAt(randomIndex++), recipesIndex, slotIndex, animate);
    
    private void OnPieceBuilt() => CompleteRecipe();
    
    private void CompleteRecipe()
    {
        recipes[firstRecipeIndex].ValidateRecipe();
        
        for (int i = 1; i < recipeSlots.Length; i++)
        {
            int cyclicIndex = (firstRecipeIndex + i) % recipeSlots.Length;
            recipes[cyclicIndex].MoveToRecipeSlot(recipeSlots[i - 1]);
        }
        
        AddRandomRecipe(firstRecipeIndex, recipeSlots.Length - 1, true);
        firstRecipeIndex = (firstRecipeIndex + 1) % recipeSlots.Length;
        
        FlashValidateColor();
    }
    
    private void OnTriedBuildingWithIncorrectItemType()
    {
        CurrentRecipe.InvalidateRecipe();
        FlashInvalidateColor();
    }

    private void FlashValidateColor() => FlashColor(validateColor);
    private void FlashInvalidateColor() => FlashColor(invalidateColor);
    private async void FlashColor(Color flashColor)
    {
        if (colorTweenHandle.IsActive())
            colorTweenHandle.Cancel();

        try
        {
            colorTweenHandle = LMotion.Create(mainPanelToColorize.color, flashColor, colorFlashDuration)
                .WithEase(Ease.OutQuad)
                .BindToColor(mainPanelToColorize);
            
            await colorTweenHandle;

            colorTweenHandle = LMotion.Create(flashColor, flashColor, colorStayDuration)
                .BindToColor(mainPanelToColorize);
            
            await colorTweenHandle;

            colorTweenHandle = LMotion.Create(flashColor, Color.white, colorFlashDuration)
                .WithEase(Ease.InQuad)
                .BindToColor(mainPanelToColorize);
            
            await colorTweenHandle;
        }
        catch (OperationCanceledException)
        {
        }
    }
    
    private void OnDisable()
    {
        Tower.PieceBuilt -= OnPieceBuilt;
        Tower.TriedBuildingWithIncorrectItemType -= OnTriedBuildingWithIncorrectItemType;

        if (colorTweenHandle.IsActive())
        {
            colorTweenHandle.Cancel();
        }
        if (mainPanelToColorize != null)
        {
            mainPanelToColorize.color = Color.white; 
        }
    }
}