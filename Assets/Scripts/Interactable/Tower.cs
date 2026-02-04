using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tower : Interactable
{
    public int Height { get; private set; }
    public float LastPlacedTime { get; private set; } = float.MaxValue;
    
    [Header("Tower")]
    
    [SerializeField] private Vector3 blockOffset;
    [SerializeField] private TextMeshProUGUI heightText;

    [SerializeField] private GameObject strawTowerPiecePrefab;
    [SerializeField] private GameObject woodTowerPiecePrefab;
    [SerializeField] private GameObject brickTowerPiecePrefab;

    private readonly List<GameObject> towerPieces = new();
    
    private Dictionary<Item.Type, GameObject> towerPieceMap;
    private Dictionary<Item.Type, GameObject> TowerPieceMap
    {
        get
        {
            towerPieceMap ??= new Dictionary<Item.Type, GameObject>
            {
                { Item.Type.Straw, strawTowerPiecePrefab },
                { Item.Type.WoodPlank, woodTowerPiecePrefab },
                { Item.Type.Brick, brickTowerPiecePrefab },
            };
            return towerPieceMap;
        }
    }
    
    [SerializeField] private RecipesList recipesList;
    public event Action PieceBuilt;

    protected override void OnGameAboutToStart()
    {
        base.OnGameAboutToStart();
        ResetTower();
    }

    protected override bool CanInteractPrimary(Player player)
    {
        // Check if the player is holding the correct item for the recipe
        return player.IsHolding && recipesList.CurrentNeededItemType == player.HeldItem.ItemType;
    }

    protected override void InteractPrimary(Player player)
    {
        // The way we display tower pieces stacking up is just by adding pieces with a certain offset everytime,
        // and with the way Unity handles rendering, the new object is rendered on top of the old one
        
        if (!TowerPieceMap.TryGetValue(player.HeldItem.ItemType, out GameObject towerPiece))
        {
            Debug.LogError("Could not find tower piece associated with " + player.HeldItem.ItemType + " held item");
            return;
        }

        GameObject towerPieceInstance = Instantiate(towerPiece, transform.position + blockOffset * Height, Quaternion.identity, transform);
        towerPieces.Add(towerPieceInstance);
        Height++;
        LastPlacedTime = LevelManager.Instance.LevelTimer;
        player.ConsumeCurrentItem();
        UpdateText();
        PieceBuilt?.Invoke();
    }

    private void UpdateText() => heightText.text = Height.ToString();
    
    private void ResetTower()
    {
        foreach (GameObject towerPiece in towerPieces)
            Destroy(towerPiece);
        towerPieces.Clear();
        Height = 0;
        LastPlacedTime = float.MaxValue;
        UpdateText();
    }
}
