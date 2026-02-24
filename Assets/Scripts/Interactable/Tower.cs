using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tower : Interactable
{
    public int Height => towerPieces.Count;
    public float LastPlacedTime { get; private set; } = float.MaxValue;
    
    [Header("Tower")]
    [SerializeField] private Vector3 blockOffset;
    [SerializeField] private TMP_Text onTowerHeightText;
    [SerializeField] private RectTransform onTowerCanvas;
    [SerializeField] private GameObject strawTowerPiecePrefab;
    [SerializeField] private GameObject woodTowerPiecePrefab;
    [SerializeField] private GameObject brickTowerPiecePrefab;

    [SerializeField] private AudioSource audioSource;

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

    public event Action TriedBuildingWithIncorrectItemType;
    public event Action PieceBuilt;
    private bool isLeftTower => this == WorldLinker.Instance.towerLeft;
    private RecipesList recipesList => isLeftTower ? CanvasLinker.Instance.recipesListLeft : CanvasLinker.Instance.recipesListRight;
    private RectTransform offTowerCanvas => isLeftTower ? CanvasLinker.Instance.offTowerHeightCanvasLeft : CanvasLinker.Instance.offTowerHeightCanvasRight;
    private TMP_Text offTowerHeightText => isLeftTower ? CanvasLinker.Instance.offTowerHeightTextLeft : CanvasLinker.Instance.offTowerHeightTextRight;

    protected override void OnGameAboutToStart()
    {
        base.OnGameAboutToStart();
        ResetTower();
    }

    public override bool CanInteract(Player player)
    {
        // Check if the player is holding the correct item for the recipe
        return player.IsHolding;
    }

    private bool IsItemCorrect(Player player) => recipesList.CurrentNeededItemType == player.HeldItem.ItemType;
    
    public override void Interact(Player player)
    {
        if (!IsItemCorrect(player))
        {
            TriedBuildingWithIncorrectItemType?.Invoke();
            return;
        }
        
        // The way we display tower pieces stacking up is just by adding pieces with a certain offset everytime,
        // and with the way Unity handles rendering, the new object is rendered on top of the old one
        
        if (!TowerPieceMap.TryGetValue(player.HeldItem.ItemType, out GameObject towerPiece))
        {
            Debug.LogError("Could not find tower piece associated with " + player.HeldItem.ItemType + " held item");
            return;
        }

        audioSource.Play();

        GameObject towerPieceInstance = Instantiate(towerPiece, transform.position + blockOffset * Height, Quaternion.identity, transform);
        towerPieces.Add(towerPieceInstance);
        LastPlacedTime = LevelManager.Instance.LevelTimer;

        if (player.HeldItem.originallyCollectedByTeam != player.PlayerTeam.CurrentTeam)
            player.PlayerStats.stolenItems++;
        player.PlayerStats.itemsPlaced++;

        player.ConsumeCurrentItem();

        UpdateTowerTopUI();
        PieceBuilt?.Invoke();
    }

    public override float GetInteractionTime() => 0;

    private void UpdateTowerTopUI()
    {
        if (Height > 0)
            onTowerCanvas.position = towerPieces[^1].transform.position + blockOffset;
        else
            onTowerCanvas.position = gameObject.transform.position;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, onTowerCanvas.position);

        if (screenPoint.y > Screen.height)
        {
            //onTowerCanvas.gameObject.SetActive(false); //remove the gray bases of the tower if u want
            
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

        onTowerHeightText.text = Height.ToString();
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
