using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasLinker : MonoBehaviour
{
    public static CanvasLinker Instance;

    public TMP_Text timerDisplay;
    public TMP_Text waitingText;
    public OffTowerCounter offTowerHeightCounterLeft;
    public OffTowerCounter offTowerHeightCounterRight;
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

    private Dictionary<PlayerTeam.Team, OffTowerCounter> offTowerHeightCounterMap;
    public Dictionary<PlayerTeam.Team, OffTowerCounter> OffTowerHeightCounterMap
    {
        get
        {
            offTowerHeightCounterMap ??= new Dictionary<PlayerTeam.Team, OffTowerCounter>
            {
                [PlayerTeam.Team.Left] = offTowerHeightCounterLeft,
                [PlayerTeam.Team.Right] = offTowerHeightCounterRight
            };
            return offTowerHeightCounterMap;
        }
    }
}
