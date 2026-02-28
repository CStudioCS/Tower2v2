using UnityEngine;
using LitMotion;
using System.Collections;
using System.Collections.Generic;

public class EndTowerScreen : MonoBehaviour
{
    [SerializeField] private Vector2 dropdownOffset;
    [SerializeField] private float towerBaseYPos;
    [SerializeField] private float towerPieceVerticalOffset;
    [SerializeField] private float initLeftTowerPiecesOffset;

    [SerializeField] private float dropdownTime;
    [SerializeField] private float towerPieceScrollTime;
    [SerializeField] private float towerPieceWaitTime;
    [SerializeField] private float inBetweenWaitTime;

    [SerializeField] private RectTransform leftTowerUI;
    [SerializeField] private RectTransform rightTowerUI;
    [SerializeField] private RectTransform brickTowerPieceUIPrefab;
    [SerializeField] private RectTransform strawTowerPieceUIPrefab;
    [SerializeField] private RectTransform woodTowerPieceUIPrefab;

    private Dictionary<Item.Type, RectTransform> towerPiecesUI;

    private bool doOnceDebug;

    private void Awake()
    {
        towerPiecesUI = new Dictionary<Item.Type, RectTransform>
        {
            { Item.Type.Straw, strawTowerPieceUIPrefab },
            { Item.Type.Brick, brickTowerPieceUIPrefab },
            { Item.Type.WoodPlank, woodTowerPieceUIPrefab },
        };
    }

    private void LateUpdate()
    {
        if (doOnceDebug) return;
        doOnceDebug = true;

        ItemRandomizer.Instance.GetAt(15);
        StartCoroutine(Dropdown(17, 11)); //debug
    }

    public IEnumerator Dropdown(int scoreBlue, int scoreRed)
    {
        yield return LMotion.Create(dropdownOffset, Vector2.zero, dropdownTime).WithEase(Ease.OutCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();

        //yield return new WaitForSeconds(1);

        bool blueWon = scoreBlue >= scoreRed;
        int minScore = blueWon ? scoreRed : scoreBlue;

        for (int i = 0; i < minScore; i++)
        {
            Item.Type towerPieceType = ItemRandomizer.Instance.GetAt(i);

            RectTransform leftTowerPiece = Instantiate(towerPiecesUI[towerPieceType], leftTowerUI);
            RectTransform rightTowerPiece = Instantiate(towerPiecesUI[towerPieceType], rightTowerUI);

            LMotion.Create(new Vector2(initLeftTowerPiecesOffset, towerBaseYPos + i * towerPieceVerticalOffset), new Vector2(0, towerBaseYPos + i * towerPieceVerticalOffset), towerPieceScrollTime).WithEase(Ease.OutCubic).Bind(
                (v) => {
                    leftTowerPiece.anchoredPosition = v;
                    rightTowerPiece.anchoredPosition = new Vector2(-v.x, v.y);
                });

            yield return new WaitForSeconds(towerPieceWaitTime);
        }

        yield return new WaitForSeconds(inBetweenWaitTime);

        for (int i = minScore; i < scoreBlue; i++)
        {
            Item.Type towerPieceType = ItemRandomizer.Instance.GetAt(i);

            RectTransform leftTowerPiece = Instantiate(towerPiecesUI[towerPieceType], leftTowerUI);

            LMotion.Create(new Vector2(initLeftTowerPiecesOffset, towerBaseYPos + i * towerPieceVerticalOffset), new Vector2(0, towerBaseYPos + i * towerPieceVerticalOffset), towerPieceScrollTime).WithEase(Ease.OutCubic).Bind(
                (v) => {
                    leftTowerPiece.anchoredPosition = v;
                });

            yield return new WaitForSeconds(towerPieceWaitTime);
        }
    }


}
