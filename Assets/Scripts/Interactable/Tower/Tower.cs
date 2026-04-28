using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Tower : Interactable
{
    public int Height => towerPieces.Count;
    public float LastPlacedTime { get; private set; } = float.MaxValue;

    [Header("Tower")]
    [SerializeField] private float targetHeight = 12.5f;
    private float averageOffset;
    private float collapseMultiplier;
    private float currentMultiplier = 1f;
    [SerializeField] private TMP_Text onTowerHeightText;
    [SerializeField] private Transform onTowerFlag;
    [SerializeField] private Vector2 flagOffset;
    [SerializeField] private Vector2 flagUITransitionOffset;
    [SerializeField] private TowerPiece strawTowerPiecePrefab;
    [SerializeField] private TowerPiece woodTowerPiecePrefab;
    [SerializeField] private TowerPiece brickTowerPiecePrefab;

    [SerializeField] private Transform towerPiecesParent;
    [SerializeField] private Collider2D colliderToActivateUponBuilding;
    [SerializeField] private bool moving;

    private readonly List<TowerPiece> towerPieces = new();

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
    [SerializeField] private PlayerTeam.Team team;
    public PlayerTeam.Team Team => team;
    private RecipesList RecipesList => RecipeBannerLinker.Instance.RecipeBannerMap[team];
    private OffTowerCounter OffTowerCounter => CanvasLinker.Instance.OffTowerHeightCounterMap[team];
    
    [SerializeField] private TowerItemCatcher towerItemCatcher;
    public TowerItemCatcher TowerItemCatcher => towerItemCatcher;

    private float GetPieceHeight(Item.Type itemType)
    {
        switch (itemType)
        {
            case Item.Type.Straw:
                return strawTowerPiecePrefab.BasePieceHeight;
            case Item.Type.WoodPlank:
                return woodTowerPiecePrefab.BasePieceHeight;
            case Item.Type.Brick:
                return brickTowerPiecePrefab.BasePieceHeight;
        }
        return 0;
    }

    private float nextPieceLocalYPosition;
    private Vector2 NextPieceLocalPosition => nextPieceLocalYPosition * Vector2.up;
    private float previousPieceLocalYPosition;
    public Vector2 PreviousPieceLocalPosition => previousPieceLocalYPosition * Vector2.up;
    
    protected override void Awake()
    {
        base.Awake();
        averageOffset = (strawTowerPiecePrefab.BasePieceHeight + woodTowerPiecePrefab.BasePieceHeight + brickTowerPiecePrefab.BasePieceHeight) / 3f;
        collapseMultiplier = 1 - averageOffset / targetHeight;
    }

    private int NextPieceSortingOrder => Height;

    protected override void OnGameAboutToStart()
    {
        base.OnGameAboutToStart();
        ResetTower();
    }

    public override bool CanInteract(Player player)
    {
        if (!LevelManager.InGame)
            return false;
        // Check if the player is holding the correct item for the recipe
        bool playerIsCorrectTeam = player.PlayerTeam.CurrentTeam == Team;
        return player.IsHolding && playerIsCorrectTeam;
    }

    public bool IsItemCorrect(Item.Type itemType) => RecipesList.CurrentNeededItemType == itemType;

    public override void Interact(Player player)
    {
        if (!IsItemCorrect(player.HeldItem.ItemType))
        {
            InteractablesNetworkHub.Instance.RPC_SyncTowerError(NetworkId);
            return;
        }

        ConstructPiece(player.HeldItem.ItemType);

        if (player.HeldItem.originallyCollectedByTeam != Team)
            player.PlayerStats.StolenItems++;

        player.ConsumeCurrentItem();
    }

    public void WrongItemError()
    {
        SoundManager.instance.PlaySound("TowerWrong");
        TriedBuildingWithIncorrectItemType?.Invoke();
    }

    public void ConstructPiece(Item.Type itemType) => InteractablesNetworkHub.Instance.RPC_SyncTowerBuild(NetworkId, itemType);

    // The way we display tower pieces stacking up is just by adding pieces with a certain offset everytime,
    // and with the way Unity handles rendering, the new object is rendered on top of the old one
    public void ApplyConstructPiece(Item.Type itemType)
    {
        if (!TowerPieceMap.TryGetValue(itemType, out TowerPiece towerPiece))
        {
            Debug.LogError("Could not find tower piece associated with " + itemType + " held item");
            return;
        }

        SoundManager.instance.PlaySound("TowerBuild");

        colliderToActivateUponBuilding.enabled = true;
        TowerPiece towerPieceInstance = Instantiate(towerPiece, towerPiecesParent);
        towerPieceInstance.transform.localPosition = NextPieceLocalPosition;
        towerPieceInstance.Initialize(this, NextPieceSortingOrder);
        towerPieces.Add(towerPieceInstance);

        LastPlacedTime = LevelManager.Instance.LevelTimer;

        UpdateTowerTopUI();
        previousPieceLocalYPosition = nextPieceLocalYPosition;
        PieceBuilt?.Invoke();

        nextPieceLocalYPosition += currentMultiplier * GetPieceHeight(itemType);
        currentMultiplier *= collapseMultiplier;

        RefreshHighlight();
    }

    public override float GetInteractionTime() => 0;

    private void UpdateTowerTopUI()
    {
        onTowerFlag.localPosition = NextPieceLocalPosition;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, (Vector2)onTowerFlag.position + flagUITransitionOffset);

        if (screenPoint.y > Screen.height)
        {
            //onTowerCanvas.gameObject.SetActive(false); //remove the gray bases of the tower if u want

            OffTowerCounter.SetUIActive(true);
            OffTowerCounter.SetText(Height.ToString());
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

    public override bool CheckIfCanBeHighlighted(Player player) 
        => base.CheckIfCanBeHighlighted(player) && player.SyncHeldItemType != -1 && IsItemCorrect((Item.Type)player.SyncHeldItemType);

    private void LateUpdate()
    {
        if (moving)
            UpdateTowerTopUI();
    }

    protected override void OnReturnedToLobby()
    {
        base.OnReturnedToLobby();
        ResetTower();
    }

    private void ResetTower()
    {
        colliderToActivateUponBuilding.enabled = false;
        foreach (TowerPiece towerPiece in towerPieces)
            Destroy(towerPiece.gameObject);
        towerPieces.Clear();
        LastPlacedTime = float.MaxValue;

        previousPieceLocalYPosition = 0;
        nextPieceLocalYPosition = 0;
        currentMultiplier = 1f;

        UpdateTowerTopUI();
    }
}
