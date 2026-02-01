using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tower : Interactable
{
    public int height = 0;

    [HideInInspector] public float lastPlacedTime = float.MaxValue;
    [SerializeField] private Vector3 blockOffset;

    [SerializeField] private TMP_Text heightText;

    [SerializeField] private GameObject strawTowerPiecePrefab;
    [SerializeField] private GameObject woodTowerPiecePrefab;
    [SerializeField] private GameObject brickTowerPiecePrefab;

    private List<GameObject> towerPieces = new();
    
    private Dictionary<Resources.Type, GameObject> towerPieceMap;
    private Dictionary<Resources.Type, GameObject> TowerPieceMap
    {
        get
        {
            towerPieceMap ??= new Dictionary<Resources.Type, GameObject>
            {
                { Resources.Type.Straw, strawTowerPiecePrefab },
                { Resources.Type.WoodPlank, woodTowerPiecePrefab },
                { Resources.Type.Brick, brickTowerPiecePrefab },
            };
            return towerPieceMap;
        }
    }
    
    [SerializeField] private RecipesList recipesList;

    private void OnEnable()
    {
        LevelManager.Instance.GameStarted += OnGameStarted;
    }

    private void OnGameStarted() => ResetTower();

    public override bool CanInteract(Player player)
    {
        //check if the player is holding the correct item for the recipe
        return player.isHolding && recipesList.CurrentNeededResourceType == player.heldItem.itemType;
    }

    public override void Interact(Player player)
    {
        //The way we display wood and cement stacking up is just by adding pieces with a certain offset everytime, and with the way
        //unity handles rendering, the new object is rendered on top of the old one
        
        if(!TowerPieceMap.TryGetValue(player.heldItem.itemType, out GameObject towerPiece))
        {
            Debug.LogError("Could not find tower piece associated with " + player.heldItem.itemType + " held item");
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
        LevelManager.Instance.GameStarted -= OnGameStarted;
    }
}
