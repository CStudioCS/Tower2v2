using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipesList : MonoBehaviour
{
    [SerializeField] private Recipe[] recipePrefabs;
    [SerializeField] private RecipeSlot[] recipeSlots;
    [SerializeField] private RecipeSlot deadRecipeSlot;

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

    [SerializeField] private PlayerTeam.Team team;
    private Tower tower;
    private Tower Tower => tower ??= TowerLinker.Instance.TowerMap[team];
    [SerializeField] private Transform recipesParent;

    // The recipes array is cyclic. The index of the first recipe is dynamic and stored in firstRecipeIndex.
    // This avoids the need to shift the array when a recipe is completed.
    private Recipe[] recipes;
    private int firstRecipeIndex;
    private int randomIndex;

    private Recipe CurrentRecipe => recipes[firstRecipeIndex];
    public Item.Type CurrentNeededItemType => CurrentRecipe.Type;

    private static readonly int Visible = Animator.StringToHash("Visible");
    [SerializeField] private Animator animator;

    private void Start()
    {
        Tower.PieceBuilt += OnPieceBuilt;
        Tower.TriedBuildingWithIncorrectItemType += OnTriedBuildingWithIncorrectItemType;
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
        LevelManager.Instance.GameStarted += OnGameStarted;
        LevelManager.Instance.GameEndedOrReturnedToLobby += OnGameEndedOrReturnedToLobby;
    }

    private void OnGameAboutToStart()
    {
        randomIndex = 0;       
        firstRecipeIndex = 0;  
        InitializeRecipes();
    }

    private void InitializeRecipes()
    {
        if (recipes == null)
            recipes = new Recipe[recipeSlots.Length];
        else
            ClearRecipes();
        
        for (int i = 0; i < recipeSlots.Length; i++)
        {
            Recipe recipeInstance = AddRandomRecipe(i, i);
            recipeInstance.SetSpriteRenderersMaskInteraction(SpriteMaskInteraction.VisibleOutsideMask);
            recipeInstance.SetBannerSortOrder(recipeSlots.Length - i - 1);
        }
    }

    private void ClearRecipes()
    {
        foreach (Recipe recipe in recipes)
        {
            Destroy(recipe.gameObject);
        }
    }

    private Recipe AddRecipe(Item.Type type, int recipesIndex, int slotIndex, bool animate = false)
    {
        if (!RecipesMap.TryGetValue(type, out Recipe recipe))
        {
            Debug.LogError($"Recipe with resource type {type} was not found.");
            return null;
        }

        Recipe recipeInstance = Instantiate(recipe, recipesParent);
        recipes[recipesIndex] = recipeInstance;
        recipeInstance.Appear(recipeSlots[slotIndex % recipeSlots.Length], animate);
        return recipeInstance;
    }

    private Recipe AddRandomRecipe(int recipesIndex, int slotIndex, bool animate = false) => AddRecipe(ItemRandomizer.Instance.GetAt(randomIndex++), recipesIndex, slotIndex, animate);

    private void OnPieceBuilt() => CompleteRecipe();

    private void CompleteRecipe()
    {
        recipes[firstRecipeIndex].ValidateRecipe(deadRecipeSlot);
        
        for (int i = 1; i < recipeSlots.Length; i++)
        {
            int cyclicIndex = (firstRecipeIndex + i) % recipeSlots.Length;
            recipes[cyclicIndex].MoveToRecipeSlot(recipeSlots[i - 1]);
            recipes[cyclicIndex].IncrementBannerSortOrder();
        }
        
        AddRandomRecipe(firstRecipeIndex, recipeSlots.Length - 1, true);
        firstRecipeIndex = (firstRecipeIndex + 1) % recipeSlots.Length;
    }

    private void OnTriedBuildingWithIncorrectItemType()
    {
        CurrentRecipe.InvalidateRecipe();
    }

    private void OnDisable()
    {
        Tower.PieceBuilt -= OnPieceBuilt;
        Tower.TriedBuildingWithIncorrectItemType -= OnTriedBuildingWithIncorrectItemType;
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
    }

    private void AnimateShow(bool visible = true)
    {
        animator.SetBool(Visible, visible);
        if (visible)
            WaitForAnimToSetSpriteRenderersMaskInteraction("Show", SpriteMaskInteraction.None);
        else
            SetSpriteRenderersMaskInteraction(SpriteMaskInteraction.VisibleOutsideMask);
    }

    private void SetSpriteRenderersMaskInteraction(SpriteMaskInteraction maskInteraction)
    {
        foreach (Recipe recipe in recipes)
            recipe.SetSpriteRenderersMaskInteraction(maskInteraction);
    }

    private void WaitForAnimToSetSpriteRenderersMaskInteraction(string stateName, SpriteMaskInteraction maskInteraction)
    {
        StartCoroutine(WaitForAnimation(stateName, () =>
        {
            SetSpriteRenderersMaskInteraction(maskInteraction);
        }));
    }

    private IEnumerator WaitForAnimation(string stateName, Action callback)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName)) yield return null;
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) yield return null;
        callback?.Invoke();
    }

    private void OnGameStarted() => AnimateShow();

    private void OnGameEndedOrReturnedToLobby() => AnimateShow(false);
}