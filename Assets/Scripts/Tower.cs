using NUnit.Framework;
using TMPro;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Tower : Interactable
{
    public int height = 0;
    [SerializeField] private Vector3 blockOffset;

    [SerializeField] private TMP_Text heightText;
    [SerializeField] private GameObject woodTowerPiecePrefab;
    [SerializeField] private GameObject brickTowerPiecePrefab;
    [SerializeField] private GameObject cementTowerPiecePrefab;

    private int lastBrickPlacement;
    private int trueHeight;
    private BlockType lastBlockType = BlockType.Cement;

    public enum BlockType { Wood, Cement, Brick }
    public override bool CanInteract(Player player)
    {
        return (lastBlockType == BlockType.Cement && (player.heldItem == Player.HeldItem.Wood || player.heldItem == Player.HeldItem.Brick)) || (player.heldItem == Player.HeldItem.Cement && lastBlockType != BlockType.Cement);
    }

    public override void Interact(Player player)
    {
        if (player.heldItem == Player.HeldItem.Wood)
        {
            Instantiate(woodTowerPiecePrefab, transform.position + blockOffset * trueHeight, Quaternion.identity, transform);
            height++;
            trueHeight++;
            lastBlockType = BlockType.Wood;
        }
        else if (player.heldItem == Player.HeldItem.Brick)
        {
            Instantiate(brickTowerPiecePrefab, transform.position + blockOffset * trueHeight, Quaternion.identity, transform);
            height++;
            trueHeight++;
            lastBlockType = BlockType.Brick;
            
        }
        else if (player.heldItem == Player.HeldItem.Cement)
        {
            Instantiate(cementTowerPiecePrefab, transform.position + blockOffset * trueHeight, Quaternion.identity, transform);
            trueHeight++;
            lastBlockType = BlockType.Cement;
        }

        player.heldItem = Player.HeldItem.Nothing;
        Destroy(player.heldItemGameobject);
        player.heldItemGameobject = null;
        UpdateText();
    }

    private void UpdateText()
        => heightText.text = height.ToString();
}
