using TMPro;
using UnityEngine;

public class CanvasLinker : MonoBehaviour
{
    public static CanvasLinker Instance;

    public TMP_Text timerDisplay;
    public TMP_Text winnerText;
    public TMP_Text waitingText;
    public TMP_Text offTowerHeightTextLeft;
    public TMP_Text offTowerHeightTextRight;
    public OffTowerCounter offTowerHeightCounterLeft;
    public OffTowerCounter offTowerHeightCounterRight;
    public RecipesList recipesListLeft;
    public RecipesList recipesListRight;
    public Animator countdown;
    public CanvasGroup LobbyUI;
    public CanvasGroup InGameUI;
    public SettingsMenu settingsMenu;

    private void Awake()
    {
        if (Instance != null)
            Destroy(Instance);

        Instance = this;
    }
}
