using UnityEngine;
using LitMotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EndTowerScreen : MonoBehaviour
{
    [SerializeField] private Vector2 dropdownOffset;
    [SerializeField] private float towerBaseYPos;
    [SerializeField] private float towerPieceVerticalOffset;
    [SerializeField] private float initLeftTowerPiecesOffset;
    [SerializeField] private float towerKickOffset = 500;
    [SerializeField] private float towerKickRotation = 10;
    [SerializeField] private float towerCenterOffset = 250;
    [SerializeField] private float losingTowerFinalRotation = 180;

    [SerializeField] private float dropdownTime;
    [SerializeField] private float towerPieceScrollTime;
    [SerializeField] private float towerPieceWaitTime;
    [SerializeField] private float inBetweenWaitTime;
    [SerializeField] private float towerKickTime = 0.7f;
    [SerializeField] private float towerComeBackTime = 0.7f;

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
        StartCoroutine(Dropdown(11, 15)); //debug
    }

    public IEnumerator Dropdown(int scoreLeft, int scoreRight)
    {
        yield return LMotion.Create(dropdownOffset, Vector2.zero, dropdownTime).WithEase(Ease.OutCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();

        bool leftWon = scoreLeft >= scoreRight;
        int minScore = leftWon ? scoreRight : scoreLeft;

        for (int i = 0; i < minScore; i++)
        {
            Item.Type towerPieceType = ItemRandomizer.Instance.GetAt(i);

            ScrollTowerPiece(towerPieceType, true, i);
            ScrollTowerPiece(towerPieceType, false, i);

            yield return new WaitForSeconds(towerPieceWaitTime);
        }

        yield return new WaitForSeconds(inBetweenWaitTime);

        for (int i = minScore; i < (leftWon ? scoreLeft : scoreRight); i++)
        {
            ScrollTowerPiece(ItemRandomizer.Instance.GetAt(i), leftWon, i);
            yield return new WaitForSeconds(towerPieceWaitTime);
        }

        RectTransform winningTowerUI = leftWon ? leftTowerUI : rightTowerUI;
        RectTransform losingTowerUI = leftWon ? rightTowerUI : leftTowerUI;
        int mirrorMult = leftWon ? 1 : -1;

        //kick
        LMotion.Create(winningTowerUI.eulerAngles.z, mirrorMult * towerKickRotation, towerKickTime).WithEase(Ease.InBack).Bind((r) => winningTowerUI.rotation = Quaternion.Euler(0, 0, r));
        yield return LMotion.Create(leftTowerUI.anchoredPosition, new Vector2(mirrorMult * towerKickOffset, 0), towerKickTime).WithEase(Ease.InBack).Bind((v) => winningTowerUI.anchoredPosition = v).ToYieldInstruction();

        //comes to middle
        leftTowerUI.GetComponent<RectMask2D>().enabled = false;
        rightTowerUI.GetComponent<RectMask2D>().enabled = false;

        LMotion.Create(losingTowerUI.eulerAngles.z, mirrorMult * losingTowerFinalRotation, towerComeBackTime).WithEase(Ease.OutQuad).Bind((r) => losingTowerUI.rotation = Quaternion.Euler(0, 0, r));
        LMotion.Create(losingTowerUI.anchoredPosition, new Vector2(mirrorMult * towerCenterOffset, 0), towerComeBackTime).Bind((v) => losingTowerUI.anchoredPosition = v);

        LMotion.Create(winningTowerUI.eulerAngles.z, 0, towerComeBackTime).WithEase(Ease.OutQuad).Bind((r) => winningTowerUI.rotation = Quaternion.Euler(0, 0, mirrorMult * r));
        yield return LMotion.Create(winningTowerUI.anchoredPosition, new Vector2(mirrorMult * towerCenterOffset, 0), towerComeBackTime).WithEase(Ease.OutQuad).Bind((v) => winningTowerUI.anchoredPosition = v).ToYieldInstruction();

        leftTowerUI.GetComponent<RectMask2D>().enabled = true;
        rightTowerUI.GetComponent<RectMask2D>().enabled = true;

        //TODO: FIX (ça me pète les couilles j'aurais pas du faire ça comme ça bref pg)


        //Transition into next screen
    }

    private void ScrollTowerPiece(Item.Type type, bool left, int index)
    {
        Item.Type towerPieceType = ItemRandomizer.Instance.GetAt(index);

        RectTransform towerPiece = Instantiate(towerPiecesUI[towerPieceType], left ? leftTowerUI : rightTowerUI);

        LMotion.Create(new Vector2(initLeftTowerPiecesOffset, towerBaseYPos + index * towerPieceVerticalOffset), new Vector2(0, towerBaseYPos + index * towerPieceVerticalOffset), towerPieceScrollTime).WithEase(Ease.OutCubic).Bind(
            (v) => {
                if(left)
                    towerPiece.anchoredPosition = v;
                else
                    towerPiece.anchoredPosition = new Vector2(-v.x, v.y);
            });
    }
}
