using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tower : Interactable
{
    public int height;

    [HideInInspector] public float lastPlacedTime = float.MaxValue;
    [SerializeField] private Vector3 blockOffset;

    [SerializeField] private TMP_Text heightText;

    [SerializeField] private GameObject strawTowerPiecePrefab;
    [SerializeField] private GameObject woodTowerPiecePrefab;
    [SerializeField] private GameObject brickTowerPiecePrefab;

    private readonly List<GameObject> towerPieces = new();
    
    private Dictionary<WorldResources.Type, GameObject> towerPieceMap;
    private Dictionary<WorldResources.Type, GameObject> TowerPieceMap
    {
        get
        {
            towerPieceMap ??= new Dictionary<WorldResources.Type, GameObject>
            {
                { WorldResources.Type.Straw, strawTowerPiecePrefab },
                { WorldResources.Type.WoodPlank, woodTowerPiecePrefab },
                { WorldResources.Type.Brick, brickTowerPiecePrefab },
            };
            return towerPieceMap;
        }
    }
    
    [SerializeField] private RecipesList recipesList;

    private void OnEnable()
    {
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
    }

    private void OnGameAboutToStart() => ResetTower();

    protected override bool CanInteractPrimary(Player player)
    {
        // Check if the player is holding the correct item for the recipe
        return player.IsHolding && recipesList.CurrentNeededResourceType == player.HeldItem.itemType;
    }

    protected override void InteractPrimary(Player player)
    {
        // The way we display wood and cement stacking up is just by adding pieces with a certain offset everytime,
        // and with the way Unity handles rendering, the new object is rendered on top of the old one
        
        if (!TowerPieceMap.TryGetValue(player.HeldItem.itemType, out GameObject towerPiece))
        {
            Debug.LogError("Could not find tower piece associated with " + player.HeldItem.itemType + " held item");
            return;
        }

        GameObject towerPieceInstance = Instantiate(towerPiece, transform.position + blockOffset * height, Quaternion.identity, transform);
        towerPieces.Add(towerPieceInstance);
        height++;

        lastPlacedTime = LevelManager.Instance.LevelTimer;

        player.ConsumeCurrentItem();

        UpdateText();
        recipesList.OnRecipeCompleted();
    }

    private void UpdateText() => heightText.text = height.ToString();
    
    private void ResetTower()
    {
        foreach (GameObject towerPiece in towerPieces)
            Destroy(towerPiece);
        towerPieces.Clear();
        height = 0;
        lastPlacedTime = float.MaxValue;
        UpdateText();
    }

    private void OnDestroy()
    {
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
    }
}
