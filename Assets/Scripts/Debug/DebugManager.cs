#if DEBUG
using System.Collections;
using System.Reflection;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager : MonoBehaviour
{
    private void Update()
    {
        // Invoke methods with hotkey attribute dynamically
        MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (MethodInfo method in methods)
        {
            object[] attr = method.GetCustomAttributes(typeof(HotkeyAttribute), false);
            if (attr.Length > 0)
            {
                HotkeyAttribute hotkey = (HotkeyAttribute)attr[0];
                if (Input.GetKeyDown(hotkey.Key))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    object[] defaultParams = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        defaultParams[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                    }

                    method.Invoke(this, defaultParams);
                }
            }
        }
    }

    [Hotkey(KeyCode.Z, "Join keyboard players")]
    public void JoinKeyboardPlayers()
    {
        if (LobbyManager.Instance == null) return;

        MethodInfo method = typeof(LobbyManager).GetMethod("JoinKeyboardPlayer", BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null) return;

        void JoinAndSetReady(PlayerBadge.ControlSchemes scheme)
        {
            PlayerInput playerInput = (PlayerInput)method.Invoke(LobbyManager.Instance, new object[] { scheme });
            SetPlayerReady(playerInput);
        }

        JoinAndSetReady(PlayerBadge.ControlSchemes.WASD);
        JoinAndSetReady(PlayerBadge.ControlSchemes.IJKL);
        JoinAndSetReady(PlayerBadge.ControlSchemes.TFGH);
        JoinAndSetReady(PlayerBadge.ControlSchemes.ArrowKeys);

        Debug.Log("Joined 4 keyboard players and set to ready.");
    }

    private void SetPlayerReady(PlayerInput playerInput)
    {
        if (playerInput == null) return;

        PlayerControlBadge playerControlBadge = playerInput.GetComponent<Player>()?.PlayerControlBadge;
        if (playerControlBadge == null) return;

        MethodInfo setReadyMethod = playerControlBadge.GetType().GetMethod("SetReady", BindingFlags.Instance | BindingFlags.NonPublic);
        if (setReadyMethod == null) return;

        setReadyMethod.Invoke(playerControlBadge, new object[] { true, true });
    }

    [Hotkey(KeyCode.X, "Increment timer by 15 seconds")]
    public void IncrementTimer(float increment = -15f)
    {
        if (LevelManager.InGame && LevelManager.Instance.HasStateAuthority)
        {
            float currentRemainingTime = LevelManager.Instance.LevelEndTimer.RemainingTime(LevelManager.Instance.Runner) ?? 0f;
            float newRemainingTime = Mathf.Max(0.1f, currentRemainingTime - increment);

            LevelManager.Instance.LevelEndTimer = TickTimer.CreateFromSeconds(LevelManager.Instance.Runner, newRemainingTime);
            Debug.Log($"Timer incremented by {increment} seconds. New timer: {newRemainingTime:F2}s");
        }
        else if (!LevelManager.Instance.HasStateAuthority)
        {
            Debug.LogWarning("Cannot increment timer - You do not have StateAuthority (Host)");
        }
    }

    [Hotkey(KeyCode.C, "Construct left tower piece")]
    public void ConstructPieceOnLeftTower()
    {
        if (NetworkManager.Instance?.Runner?.IsServer != true) return;

        if (TowerLinker.Instance?.TowerMap[PlayerTeam.Team.Left] != null)
        {
            TowerLinker.Instance.TowerMap[PlayerTeam.Team.Left].ConstructPiece(RecipeBannerLinker.Instance.RecipeBannerMap[PlayerTeam.Team.Left].CurrentNeededItemType);
            Debug.Log("Constructed piece on left tower");
        }
    }

    [Hotkey(KeyCode.V, "Construct right tower piece")]
    public void ConstructPieceOnRightTower()
    {
        if (NetworkManager.Instance?.Runner?.IsServer != true) return;

        if (TowerLinker.Instance.TowerMap[PlayerTeam.Team.Right] != null)
        {
            TowerLinker.Instance.TowerMap[PlayerTeam.Team.Right].ConstructPiece(RecipeBannerLinker.Instance.RecipeBannerMap[PlayerTeam.Team.Right].CurrentNeededItemType);
            Debug.Log("Constructed piece on right tower");
        }
    }

    [Hotkey(KeyCode.B, "End Game instantly with high score")]
    public void EndGameInstantly()
    {
        if (NetworkManager.Instance?.Runner?.IsServer != true) return;

        StartCoroutine(EndGameInstantlyRoutine());
    }

    private IEnumerator EndGameInstantlyRoutine(int scoreLeft = 30, int scoreRight = 18)
    {
        JoinKeyboardPlayers();

        yield return new WaitForSeconds(3);

        for (int i = 0; i < scoreLeft; i++)
            ConstructPieceOnLeftTower();
        for (int i = 0; i < scoreRight; i++)
            ConstructPieceOnRightTower();

        yield return null;

        IncrementTimer((LevelManager.Instance.LevelEndTimer.RemainingTime(LevelManager.Instance.Runner) ?? 0f) - 0.5f);
    }

    [Hotkey(KeyCode.N, "Set time scale to 4 or back to 1")]
    public void ToggleTimeScale(float newTimeScale = 4f)
    {
        if (Mathf.Approximately(Time.timeScale, 1f))
            Time.timeScale = newTimeScale;
        else if (!Mathf.Approximately(Time.timeScale, 0f))
            Time.timeScale = 1f;
    }

    [Hotkey(KeyCode.M, "Toggle no clip for all players")]
    public void ToggleNoClip()
    {
        foreach (Player player in GameStartManager.Instance.Players)
        {
            Collider2D[] colliders = player.GetComponentsInChildren<Collider2D>();
            foreach (Collider2D collider in colliders)
            {
                if (!collider.isTrigger)
                    collider.enabled = !collider.enabled;
            }
        }
    }

    [Hotkey(KeyCode.Comma, "Set timer to max")]
    public void SetTimerToMax()
    {
        if (LevelManager.InGame && LevelManager.Instance.HasStateAuthority)
            LevelManager.Instance.LevelEndTimer = TickTimer.CreateFromSeconds(LevelManager.Instance.Runner, LevelManager.Instance.TimerLimit);
    }

    [Hotkey(KeyCode.Period, "Set time scale to 0.1 or back to 1")]
    public void ToggleTimeScaleSlow() => ToggleTimeScale(.1f);
}
#endif