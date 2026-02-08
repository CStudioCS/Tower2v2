using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CanvasLinker : MonoBehaviour
{
    public static CanvasLinker Instance;

    public TMP_Text timerDisplay;
    public TMP_Text winnerText;
    public TMP_Text waitingText;
    public TMP_Text offTowerHeightTextLeft;
    public TMP_Text offTowerHeightTextRight;
    public RectTransform offTowerHeightCanvasLeft;
    public RectTransform offTowerHeightCanvasRight;
    public RecipesList recipesListLeft;
    public RecipesList recipesListRight;
    public Animator countdown;
    public GameObject MenuUI;
    public GameObject InGameUI;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }
}
