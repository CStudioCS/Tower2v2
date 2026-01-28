using TMPro;
using UnityEngine;

public class Tower : Interactable
{
    public int height = 0; //The actual height of the tower (doesn't augment when placing cement)

    [SerializeField] private Vector3 blockOffset;

    [SerializeField] private TMP_Text heightText;

    [SerializeField] private GameObject woodTowerPiecePrefab;
    [SerializeField] private GameObject brickTowerPiecePrefab;
    [SerializeField] private GameObject cementTowerPiecePrefab;

    private int trueHeight; //The height of the tower if adding cement is included as a height increase
    private BlockType lastBlockType = BlockType.Cement;

    public enum BlockType { Wood, Cement, Brick }
    public override bool CanInteract(Player player)
    {
        //check if the order of BlockTypes is correct (Wood => Cement => Wood or Brick => Cement => ...)
        return (lastBlockType == BlockType.Cement && (player.heldItem == Player.HeldItem.Wood || player.heldItem == Player.HeldItem.Brick)) || (player.heldItem == Player.HeldItem.Cement && lastBlockType != BlockType.Cement);
    }

    public override void Interact(Player player)
    {
        //The way we display wood and cement stacking up is just by adding pieces with a certain offset everytime, and with the way
        //unity handles rendering, the new object is rendered on top of the old one
        if (player.heldItem == Player.HeldItem.Wood)
        {
            Instantiate(woodTowerPiecePrefab, transform.position + blockOffset * trueHeight, Quaternion.identity, transform);
            height++;
            lastBlockType = BlockType.Wood;
        }
        else if (player.heldItem == Player.HeldItem.Brick)
        {
            Instantiate(brickTowerPiecePrefab, transform.position + blockOffset * trueHeight, Quaternion.identity, transform);
            height++;
            lastBlockType = BlockType.Brick;
            
        }
        else if (player.heldItem == Player.HeldItem.Cement)
        {
            Instantiate(cementTowerPiecePrefab, transform.position + blockOffset * trueHeight, Quaternion.identity, transform);
            lastBlockType = BlockType.Cement;
        }

        trueHeight++;
        player.heldItem = Player.HeldItem.Nothing;
        Destroy(player.heldItemGameobject);
        player.heldItemGameobject = null;
        UpdateText();
    }

    private void UpdateText()
        => heightText.text = height.ToString();
}
