using System;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private Transform onTowerFlag;
    [SerializeField] private Vector2 flagOffset;
    [SerializeField] private TowerPiece strawTowerPiecePrefab;
    [SerializeField] private TowerPiece woodTowerPiecePrefab;
    [SerializeField] private TowerPiece brickTowerPiecePrefab;

    [SerializeField] private Transform towerPiecesParent;
    [SerializeField] private Collider2D colliderToActivateUponBuilding;

    private readonly List<TowerPiece> towerPieces = new();

    private Item.Type lastBlockType;

    private Vector2 newPieceLocalPosition;
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
    public PlayerTeam.Team TowerTeam => IsLeftTower? PlayerTeam.Team.Left : PlayerTeam.Team.Right;
    private RecipesList RecipesList => IsLeftTower ? CanvasLinker.Instance.recipesListLeft : CanvasLinker.Instance.recipesListRight;
    private OffTowerCounter OffTowerCounter => IsLeftTower ? CanvasLinker.Instance.offTowerHeightCounterLeft : CanvasLinker.Instance.offTowerHeightCounterRight;
    private TMP_Text OffTowerHeightText => IsLeftTower ? CanvasLinker.Instance.offTowerHeightTextLeft : CanvasLinker.Instance.offTowerHeightTextRight;

    public Vector2 blockOffset => Height == 0 ? Vector2.zero : lastBlockType switch
    {
        Item.Type.Brick => onBrickBlockOffset,
        Item.Type.WoodPlank => onWoodBlockOffset,
        Item.Type.Straw => onStrawBlockOffset,
        _ => Vector2.zero
    };

    public Vector2 PreviousPieceLocalPosition => newPieceLocalPosition - blockOffset;

    private int NextPieceSortingOrder => Height;

    protected override void OnGameAboutToStart()
    {
        base.OnGameAboutToStart();
        newPieceLocalPosition = Vector2.zero;
        ResetTower();
    }

    public override bool CanInteract(Player player)
    {
        if (!LevelManager.InGame)
            return false;
        // Check if the player is holding the correct item for the recipe
        bool playerIsCorrectTeam = player.PlayerTeam.CurrentTeam == TowerTeam;
        return player.IsHolding && playerIsCorrectTeam;
    }

    public bool IsItemCorrect(Item item) => RecipesList.CurrentNeededItemType == item.ItemType;

    public override void Interact(Player player)
    {
        if (!IsItemCorrect(player.HeldItem))
        {
            SoundManager.instance.PlaySound("TowerWrong");
            TriedBuildingWithIncorrectItemType?.Invoke();
            return;
        }

        ConstructPiece(player.HeldItem);

        player.ConsumeCurrentItem();
    }

    // The way we display tower pieces stacking up is just by adding pieces with a certain offset everytime,
    // and with the way Unity handles rendering, the new object is rendered on top of the old one
    public void ConstructPiece(Item item)
    {
        if (!TowerPieceMap.TryGetValue(item.ItemType, out TowerPiece towerPiece))
        {
            Debug.LogError("Could not find tower piece associated with " + item.ItemType + " held item");
            return;
        }

        SoundManager.instance.PlaySound("TowerBuild");

        colliderToActivateUponBuilding.enabled = true;
        newPieceLocalPosition += blockOffset;
        TowerPiece towerPieceInstance = Instantiate(towerPiece, towerPiecesParent);
        towerPieceInstance.transform.localPosition = newPieceLocalPosition;
        towerPieceInstance.Initialize(this, NextPieceSortingOrder);
        towerPieces.Add(towerPieceInstance);
        lastBlockType = item.ItemType;
        LastPlacedTime = LevelManager.Instance.LevelTimer;
        UpdateTowerTopUI();
        PieceBuilt?.Invoke();
    }

    public override float GetInteractionTime() => 0;

    private void UpdateTowerTopUI()
    {
        onTowerFlag.localPosition = newPieceLocalPosition;
        // if (Height > 0)
        //     onTowerFlag.position = (Vector2)towerPieces[^1].transform.position + blockOffset + flagOffset;
        // else
        //     onTowerFlag.position = (Vector2)gameObject.transform.position + flagOffset;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, onTowerFlag.position);

        if (screenPoint.y > Screen.height)
        {
            //onTowerCanvas.gameObject.SetActive(false); //remove the gray bases of the tower if u want

            OffTowerCounter.SetUIActive(true);
            
            OffTowerHeightText.text = Height.ToString();
            OffTowerCounter.transform.position = new Vector3(screenPoint.x, OffTowerCounter.transform.position.y, 0);
        }
        else
        {
            OffTowerCounter.SetUIActive(false);

            onTowerFlag.gameObject.SetActive(true);
            onTowerHeightText.text = Height.ToString();
        }

        onTowerHeightText.text = Height.ToString();
    }

    public override bool CheckIfCanBeHighlighted(Player player) => base.CheckIfCanBeHighlighted(player) && player.IsHolding && IsItemCorrect(player.HeldItem);

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
