using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tower : Interactable
{
    public int height = 0; //The actual height of the tower (doesn't augment when placing cement)

    [HideInInspector] public float lastPlacedTime = float.MaxValue;
    [SerializeField] private Vector3 blockOffset;

    [SerializeField] private TMP_Text heightText;

    [SerializeField] private GameObject strawTowerPiecePrefab;
    [SerializeField] private GameObject woodTowerPiecePrefab;
    [SerializeField] private GameObject brickTowerPiecePrefab;

    private Dictionary<Resources.Type, GameObject> towerPieceMap = new();
    
    private Resources.Type lastBlockType;
    
    [SerializeField] private RecipesList recipesList;
    
    private void Awake()
    {
        towerPieceMap = new Dictionary<Resources.Type, GameObject>
        {
            { Resources.Type.Straw, strawTowerPiecePrefab },
            { Resources.Type.WoodPlank, woodTowerPiecePrefab },
            { Resources.Type.Brick, brickTowerPiecePrefab },
        };
    }
    
    public override bool CanInteract(Player player)
    {
        //check if the player is holding the correct item for the recipe
        return recipesList.CurrentNeededResourceType == player.heldItem.itemType;
    }

    public override void Interact(Player player)
    {
        //The way we display wood and cement stacking up is just by adding pieces with a certain offset everytime, and with the way
        //unity handles rendering, the new object is rendered on top of the old one
        
        if(!towerPieceMap.TryGetValue(player.heldItem.itemType, out GameObject towerPiece))
        {
            Debug.LogError("Could not find tower piece associated with " + player.heldItem.itemType + " held item");
            return;
        }

        Instantiate(towerPiece, transform.position + blockOffset * height, Quaternion.identity, transform);
        height++;
        lastBlockType = player.heldItem.itemType;

        lastPlacedTime = LevelManager.instance.levelTimer;

        player.ConsumeCurrentItem();

        UpdateText();
        recipesList.OnRecipeCompleted();
    }

    private void UpdateText()
        => heightText.text = height.ToString();
}
