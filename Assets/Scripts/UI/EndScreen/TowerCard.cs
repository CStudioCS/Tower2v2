using System.Collections;
using System.Collections.Generic;
using LitMotion;
using TMPro;
using UnityEngine;

public class TowerCard : MonoBehaviour
{
    [Header("Position Data")]
    [SerializeField] private float towerBaseYPos;
    [SerializeField] private float defaultTowerPieceVerticalOffset;
    [SerializeField] private float initLeftTowerPiecesOffset;

    [SerializeField] private float maxTowerPieceVerticalPosition;
    [SerializeField] private float packingProportion = 2f / 3f;

    [Header("Times")]
    [SerializeField] private float towerPieceScrollTime;
    [SerializeField] private float towerPieceWaitTime;
    [SerializeField] private float inBetweenWaitTime;

    [Header("References")]
    [SerializeField] private Animator towerAnimator;
    [SerializeField] private RectTransform leftTowerUI;
    [SerializeField] private RectTransform rightTowerUI;
    [SerializeField] private RectTransform brickTowerPieceUIPrefab;
    [SerializeField] private RectTransform strawTowerPieceUIPrefab;
    [SerializeField] private RectTransform woodTowerPieceUIPrefab;
    [SerializeField] private TMP_Text leftScoreText;
    [SerializeField] private TMP_Text rightScoreText;
    [SerializeField] private CanvasGroup pressAnyButtonCanvasGroup;

    private Dictionary<Item.Type, RectTransform> towerPiecesUI;

    private void Awake()
    {
        towerPiecesUI = new Dictionary<Item.Type, RectTransform>
        {
            { Item.Type.Straw, strawTowerPieceUIPrefab },
            { Item.Type.Brick, brickTowerPieceUIPrefab },
            { Item.Type.WoodPlank, woodTowerPieceUIPrefab },
        };
    }

    public IEnumerator Dropdown()
    {
        pressAnyButtonCanvasGroup.alpha = 0f;

        int scoreLeft = WorldLinker.Instance.towerLeft.Height;
        int scoreRight = WorldLinker.Instance.towerRight.Height;

        bool leftWon = scoreLeft >= scoreRight;
        int minScore = Mathf.Min(scoreLeft, scoreRight);
        int maxScore = Mathf.Max(scoreLeft, scoreRight);

        bool draw = scoreLeft == 0 && scoreRight == 0;
        if (draw)
        {
            leftScoreText.text = "huh?\n0";
            rightScoreText.text = "draw?\n0";
        }
        else
        {
            leftScoreText.text = (leftWon ? "BLUE WINS!\n" : "\n") + scoreLeft;
            rightScoreText.text = (!leftWon ? "RED WINS!\n" : "\n") + scoreRight;
        }

        //reset
        foreach (Transform child in leftTowerUI.transform)
            Destroy(child.gameObject);
        foreach (Transform child in rightTowerUI.transform)
            Destroy(child.gameObject);


        towerAnimator.SetTrigger("Dropdown");

        yield return null; //wait one frame for animator to change state (kinda ghetto)
        yield return new WaitForSeconds(towerAnimator.GetCurrentAnimatorStateInfo(0).length);

        towerAnimator.SetTrigger("TowerStacking");

        bool packing = maxScore * defaultTowerPieceVerticalOffset > maxTowerPieceVerticalPosition - towerBaseYPos;

        float dynamicOffset = 0;
        int numOfPackedTowerPieces = 0;
        
        if (packing)
        {
            numOfPackedTowerPieces = (int)Mathf.Ceil(packingProportion * maxScore);
            dynamicOffset = 2 * ((maxTowerPieceVerticalPosition - towerBaseYPos) - (maxScore - numOfPackedTowerPieces) * defaultTowerPieceVerticalOffset) / (numOfPackedTowerPieces * (numOfPackedTowerPieces + 1)); //p
        }

        float GetOffset(int i)
        {
            if (packing)
                return i > numOfPackedTowerPieces ? defaultTowerPieceVerticalOffset : (i + 1) * dynamicOffset;
            else
                return defaultTowerPieceVerticalOffset;
        } 

        float verticalPos = towerBaseYPos;
        for (int i = 0; i < minScore; i++)
        {
            Item.Type towerPieceType = ItemRandomizer.Instance.GetAt(i);

            ScrollTowerPiece(towerPieceType, true, i, verticalPos);
            ScrollTowerPiece(towerPieceType, false, i, verticalPos);

            verticalPos += GetOffset(i);

            yield return new WaitForSeconds(towerPieceWaitTime);
        }

        if(scoreLeft != 0 || scoreRight != 0)
            yield return new WaitForSeconds(inBetweenWaitTime);

        for (int i = minScore; i < (leftWon ? scoreLeft : scoreRight); i++)
        {
            ScrollTowerPiece(ItemRandomizer.Instance.GetAt(i), leftWon, i, verticalPos);
            verticalPos += GetOffset(i);
            yield return new WaitForSeconds(towerPieceWaitTime);
        }
        
        //kick
        if (leftWon)
            towerAnimator.SetTrigger("KickToRight");
        else
            towerAnimator.SetTrigger("KickToLeft");

        if (!draw)
            SoundManager.instance.PlaySound("TowerKick");
        
        yield return null; //wait one frame for animator to change state (kinda ghetto)
        yield return new WaitForSeconds(towerAnimator.GetCurrentAnimatorStateInfo(0).length);

        towerAnimator.SetTrigger("DisplayScore");

        yield return null;
        yield return new WaitForSeconds(towerAnimator.GetCurrentAnimatorStateInfo(0).length);

        LMotion.Create(0f, 1f, .5f).Bind(value => pressAnyButtonCanvasGroup.alpha = value);

        yield return new WaitUntil(() => Input.anyKey);
    }

    private void ScrollTowerPiece(Item.Type type, bool left, int index, float verticalPos)
    {
        RectTransform towerPiece = Instantiate(towerPiecesUI[type], left ? leftTowerUI : rightTowerUI);

        LMotion.Create(new Vector2(initLeftTowerPiecesOffset, verticalPos), new Vector2(0, verticalPos), towerPieceScrollTime).WithEase(Ease.OutCubic).Bind(
            (v) => {
                if(left)
                    towerPiece.anchoredPosition = v;
                else
                    towerPiece.anchoredPosition = new Vector2(-v.x, v.y);
            });
    }
}
