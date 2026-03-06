using LitMotion;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerCard : MonoBehaviour
{
    [Header("Position Data")]
    [SerializeField] private float towerBaseYPos;
    [SerializeField] private float towerPieceVerticalOffset;
    [SerializeField] private float initLeftTowerPiecesOffset;

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
        //int scoreLeft = WorldLinker.Instance.towerLeft.Height;
        //int scoreRight = WorldLinker.Instance.towerRight.Height;
        Debug.LogError("this shit still in debug !!");
        int scoreLeft = 12;
        int scoreRight = 17;
        bool leftWon = scoreLeft >= scoreRight;
        int minScore = leftWon ? scoreRight : scoreLeft;

        leftScoreText.text = scoreLeft.ToString();
        rightScoreText.text = scoreRight.ToString();

        //reset
        foreach (Transform child in leftTowerUI.transform)
            Destroy(child.gameObject);
        foreach (Transform child in rightTowerUI.transform)
            Destroy(child.gameObject);


        towerAnimator.SetTrigger("Dropdown");

        yield return null; //wait one frame for animator to change state (kinda ghetto)
        yield return new WaitForSeconds(towerAnimator.GetCurrentAnimatorStateInfo(0).length);

        towerAnimator.SetTrigger("TowerStacking");

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
        if(leftWon)
            towerAnimator.SetTrigger("KickToRight");
        else
            towerAnimator.SetTrigger("KickToLeft");

        
        yield return null; //wait one frame for animator to change state (kinda ghetto)
        yield return new WaitForSeconds(towerAnimator.GetCurrentAnimatorStateInfo(0).length);

        towerAnimator.SetTrigger("DisplayScore");

        yield return null;
        yield return new WaitForSeconds(towerAnimator.GetCurrentAnimatorStateInfo(0).length);


        yield return new WaitUntil(() => Input.anyKey);
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
