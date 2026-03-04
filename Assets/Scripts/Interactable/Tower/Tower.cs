using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEditor.UI;
using UnityEngine;

public class Tower : Interactable
{
    public int Height => towerPieces.Count;
    public float LastPlacedTime { get; private set; } = float.MaxValue;

    [Header("Tower")]
    [SerializeField] private Vector2 onStrawBlockOffset;
    [SerializeField] private Vector2 onWoodBlockOffset;
    [SerializeField] private Vector2 onBrickBlockOffset;
    [SerializeField] private TMP_Text onTowerHeightText;
    [SerializeField] private RectTransform onTowerCanvas;
    [SerializeField] private TowerPiece strawTowerPiecePrefab;
    [SerializeField] private TowerPiece woodTowerPiecePrefab;
    [SerializeField] private TowerPiece brickTowerPiecePrefab;

    [SerializeField] private Transform towerPiecesParent;
    [SerializeField] private Collider2D colliderToActivateUponBuilding;

    [SerializeField] private AudioSource audioSource;

    private readonly List<TowerPiece> towerPieces = new();

    private Item.Type lastBlockType;

    private Vector2 NewPiecePosition;
    private Dictionary<Item.Type, TowerPiece> towerPieceMap;
    private Dictionary<Item.Type, TowerPiece> TowerPieceMap
    {
        get
        {
            towerPieceMap ??= new Dictionary<Item.Type, TowerPiece>
            {
                { Item.Type.Straw, strawTowerPiecePrefab },
                { Item.Type.WoodPlank, woodTowerPiecePrefab },
                { Item.Type.Brick, brickTowerPiecePrefab },
            };
            return towerPieceMap;
        }
    }

    public event Action TriedBuildingWithIncorrectItemType;
    public event Action PieceBuilt;
    private bool IsLeftTower => this == WorldLinker.Instance.towerLeft;
    private RecipesList RecipesList => IsLeftTower ? CanvasLinker.Instance.recipesListLeft : CanvasLinker.Instance.recipesListRight;
    private RectTransform OffTowerCanvas => IsLeftTower ? CanvasLinker.Instance.offTowerHeightCanvasLeft : CanvasLinker.Instance.offTowerHeightCanvasRight;
    private TMP_Text OffTowerHeightText => IsLeftTower ? CanvasLinker.Instance.offTowerHeightTextLeft : CanvasLinker.Instance.offTowerHeightTextRight;

    public Vector2 blockOffset => Height == 0 ? Vector2.zero : lastBlockType switch
    {
        Item.Type.Brick => onBrickBlockOffset,
        Item.Type.WoodPlank => onWoodBlockOffset,
        Item.Type.Straw => onStrawBlockOffset,
        _ => Vector2.zero
    };

    public Vector2 PreviousPiecePosition => NewPiecePosition - blockOffset;

    private int NextPieceSortingOrder => Height;

    protected override void OnGameAboutToStart()
    {
        base.OnGameAboutToStart();
        NewPiecePosition = (Vector2)transform.position;
        ResetTower();
    }

    public override bool CanInteract(Player player)
    {
        // Check if the player is holding the correct item for the recipe
        return player.IsHolding;
    }

    private bool IsItemCorrect(Player player) => RecipesList.CurrentNeededItemType == player.HeldItem?.ItemType;
    
    public override void Interact(Player player)
    {
        if (!IsItemCorrect(player))
        {
            TriedBuildingWithIncorrectItemType?.Invoke();
            return;
        }

        // The way we display tower pieces stacking up is just by adding pieces with a certain offset everytime,
        // and with the way Unity handles rendering, the new object is rendered on top of the old one

        if (!TowerPieceMap.TryGetValue(player.HeldItem.ItemType, out TowerPiece towerPiece))
        {
            Debug.LogError("Could not find tower piece associated with " + player.HeldItem.ItemType + " held item");
            return;
        }

        audioSource.Play();
        
        colliderToActivateUponBuilding.enabled = true;
        NewPiecePosition += blockOffset;
        TowerPiece towerPieceInstance = Instantiate(towerPiece, NewPiecePosition, Quaternion.identity, towerPiecesParent);
        towerPieceInstance.Initialize(this, NextPieceSortingOrder);
        towerPieces.Add(towerPieceInstance);
        lastBlockType = player.HeldItem.ItemType;
        LastPlacedTime = LevelManager.Instance.LevelTimer;

        player.ConsumeCurrentItem();
        UpdateTowerTopUI();
        PieceBuilt?.Invoke();
    }

    public override float GetInteractionTime() => 0;

    private void UpdateTowerTopUI()
    {
        if (Height > 0)
            onTowerCanvas.position = (Vector2)towerPieces[^1].transform.position + blockOffset;
        else
            onTowerCanvas.position = gameObject.transform.position;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, onTowerCanvas.position);

        if (screenPoint.y > Screen.height)
        {
            //onTowerCanvas.gameObject.SetActive(false); //remove the gray bases of the tower if u want

            OffTowerCanvas.gameObject.SetActive(true);
            OffTowerHeightText.text = Height.ToString();
            float yPos = OffTowerCanvas.position.y;
            OffTowerCanvas.position = new Vector3(screenPoint.x, yPos, 0);
        }
        else
        {
            OffTowerCanvas.gameObject.SetActive(false);
            onTowerCanvas.gameObject.SetActive(true);
            onTowerHeightText.text = Height.ToString();
        }

        onTowerHeightText.text = Height.ToString();
    }

    public override bool CheckIfCanBeHighlighted(Player player) => base.CheckIfCanBeHighlighted(player) && player.IsHolding && IsItemCorrect(player);

    private void ResetTower()
    {
        colliderToActivateUponBuilding.enabled = false;
        foreach (TowerPiece towerPiece in towerPieces)
            Destroy(towerPiece.gameObject);
        towerPieces.Clear();
        LastPlacedTime = float.MaxValue;
        UpdateTowerTopUI();
    }
}
