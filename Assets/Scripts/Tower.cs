using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tower : Interactable
{
    public int height = 0; //The actual height of the tower (doesn't augment when placing cement)

    public float lastPlacedTime = float.MaxValue;
    [SerializeField] private Vector3 blockOffset;

    [SerializeField] private TMP_Text heightText;

    [SerializeField] private GameObject strawTowerPiecePrefab;
    [SerializeField] private GameObject woodTowerPiecePrefab;
    [SerializeField] private GameObject brickTowerPiecePrefab;

    private Dictionary<Resources.Type, GameObject> towerPieceMap = new();
    
    private int trueHeight; //The height of the tower if adding cement is included as a height increase
    private Resources.Type lastBlockType;
    
    [SerializeField] private RecipesList recipesList;
    
    private void Awake()
    {
        towerPieceMap = new Dictionary<Resources.Type, GameObject>
        {
            { Resources.Type.Straw, strawTowerPiecePrefab },
            { Resources.Type.WoodLog, woodTowerPiecePrefab },
            { Resources.Type.Brick, brickTowerPiecePrefab },
        };
    }
    
    public override bool CanInteract(Player player)
    {
        //check if the player is holding the correct item for the recipe
        return recipesList.CurrentNeededResourceType == player.heldItem;
    }

    public override void Interact(Player player)
    {
        //The way we display wood and cement stacking up is just by adding pieces with a certain offset everytime, and with the way
        //unity handles rendering, the new object is rendered on top of the old one
        
        Instantiate(towerPieceMap[player.heldItem], transform.position + blockOffset * trueHeight, Quaternion.identity, transform);
        height++;
        lastBlockType = player.heldItem;

        trueHeight++;
        lastPlacedTime = LevelManager.instance.levelTimer;
        player.isHolding = false;
        Destroy(player.heldItemGameobject);
        player.heldItemGameobject = null;
        UpdateText();
        recipesList.OnRecipeCompleted();
    }

    private void UpdateText()
        => heightText.text = height.ToString();
}
