using UnityEngine;
using LitMotion;
using System.Collections;

public class EndTowerScreen : MonoBehaviour
{
    [SerializeField] private Vector2 dropdownOffset;
    [SerializeField] private float dropdownTime;
    [SerializeField] private float towerBaseYPos;
    [SerializeField] private float towerPieceVerticalOffset;
    [SerializeField] private float initLeftTowerPiecesOffset;
    [SerializeField] private float towerPieceScrollTime;

    [SerializeField] private RectTransform leftTowerUI;
    [SerializeField] private RectTransform rightTowerUI;
    [SerializeField] private RectTransform towerPieceUIPrefab;
    private void Start()
    {
        StartCoroutine(Dropdown(12, 15)); //debug
    }

    public IEnumerator Dropdown(int scoreBlue, int scoreRed)
    {
        yield return LMotion.Create(dropdownOffset, Vector2.zero, dropdownTime).WithEase(Ease.OutCubic).Bind((v) => transform.localPosition = v).ToYieldInstruction();

        yield return new WaitForSeconds(1);

        bool blueWon = scoreBlue >= scoreRed;
        int minScore = blueWon ? scoreRed : scoreBlue;

        for (int i = 0; i < minScore; i++)
        {
            RectTransform leftTowerPiece = Instantiate(towerPieceUIPrefab, leftTowerUI);
            RectTransform rightTowerPiece = Instantiate(towerPieceUIPrefab, rightTowerUI);
            LMotion.Create(new Vector2(initLeftTowerPiecesOffset, towerBaseYPos + i * towerPieceVerticalOffset), new Vector2(0, towerBaseYPos + i * towerPieceVerticalOffset), towerPieceScrollTime).WithEase(Ease.OutCubic).Bind(
                (v) => {
                    leftTowerPiece.localPosition = v;
                    rightTowerUI.localPosition = new Vector2(-v.x, v.y);
                });
            yield return new WaitForSeconds(towerPieceScrollTime * 2 / 3);
        }
    }
}
