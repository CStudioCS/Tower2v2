using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class Tower : Interactable
{
    public int Height => towerPieces.Count;
    public float LastPlacedTime { get; private set; } = float.MaxValue;
    
    [Header("Tower")]
    [SerializeField] private Vector3 blockOffset;
    [SerializeField] private TextMeshProUGUI onTowerHeightText;
    [SerializeField] private TextMeshProUGUI offTowerHeightText;
    [SerializeField] private RectTransform onTowerCanvas;
    [SerializeField] private RectTransform offTowerCanvas;
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
        LastPlacedTime = LevelManager.Instance.LevelTimer;

        player.ConsumeCurrentItem();
        UpdateTowerTopUI();
        PieceBuilt?.Invoke();
    }

private void UpdateTowerTopUI()
    {
        if (Height > 0)
        {
            onTowerCanvas.position = towerPieces[^1].transform.position + blockOffset;
        }
        else
        {
            onTowerCanvas.position = gameObject.transform.position;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, onTowerCanvas.position);

        if (screenPoint.y > Screen.height)
        {
            onTowerCanvas.gameObject.SetActive(false);
            offTowerCanvas.gameObject.SetActive(true);
            offTowerHeightText.text = Height.ToString();
            float yPos = offTowerCanvas.position.y;
            offTowerCanvas.position = new Vector3(screenPoint.x, yPos, 0);
        }
        else
        {
            offTowerCanvas.gameObject.SetActive(false);
            onTowerCanvas.gameObject.SetActive(true);
            onTowerHeightText.text = Height.ToString();
        }
    }

    private void ResetTower()
    {
        foreach (GameObject towerPiece in towerPieces)
            Destroy(towerPiece);
        towerPieces.Clear();
        LastPlacedTime = float.MaxValue;
        UpdateTowerTopUI();
    }
}
