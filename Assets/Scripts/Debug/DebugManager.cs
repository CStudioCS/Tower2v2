#if DEBUG
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class DebugManager: MonoBehaviour
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
		if (LobbyManager.Instance == null)
		{
			Debug.LogError("LobbyManager instance not found!");
			return;
		}

		MethodInfo method = typeof(LobbyManager).GetMethod("JoinKeyboardPlayer", BindingFlags.Instance | BindingFlags.NonPublic);

		if (method == null)
		{
			Debug.LogError("JoinKeyboardPlayer method not found in LobbyManager via reflection!");
			return;
		}

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
		if (playerInput == null)
		{
			Debug.LogError("PlayerInput is null. Cannot set player as ready.");
			return;
		}

		PlayerControlBadge playerControlBadge = playerInput.GetComponent<Player>()?.PlayerControlBadge;
		if (playerControlBadge == null)
		{
			Debug.LogError("PlayerControlBadge not found on PlayerInput. Cannot set player as ready.");
			return;
		}

		// Use reflection to invoke the private SetReady method
		MethodInfo setReadyMethod = playerControlBadge.GetType().GetMethod("SetReady", BindingFlags.Instance | BindingFlags.NonPublic);
		if (setReadyMethod == null)
		{
			Debug.LogError("SetReady method not found in PlayerControlBadge via reflection!");
			return;
		}

		setReadyMethod.Invoke(playerControlBadge, new object[] { true, true });
	}

	[Hotkey(KeyCode.X, "Decrement timer by 15 seconds")]
	public void IncrementTimer(float increment = -15f)
	{
		if (LevelManager.InGame)
		{
			float currentTimer = LevelManager.Instance.LevelTimer;

			// Use reflection to access the private timerLimit field
			FieldInfo timerLimitField = typeof(LevelManager).GetField("timerLimit", BindingFlags.Instance | BindingFlags.NonPublic);
			if (timerLimitField == null)
			{
				Debug.LogError("timerLimit field not found in LevelManager via reflection!");
				return;
			}

			float timerLimit = (float)timerLimitField.GetValue(LevelManager.Instance);
			float newTimer = Mathf.Clamp(currentTimer - increment, 0, timerLimit - .05f);

			// Use reflection to set the LevelTimer since it's a private set
			PropertyInfo levelTimerProperty = typeof(LevelManager).GetProperty("LevelTimer");
			levelTimerProperty?.SetValue(LevelManager.Instance, newTimer);

			Debug.Log($"Timer incremented by {increment} seconds. New timer: {newTimer:F2}s");
		}
		else
		{
			Debug.LogError("Cannot increment timer - game is not in progress or LevelManager is null");
		}
	}

	[Hotkey(KeyCode.C, "Construct left tower piece")]
	public void ConstructPieceOnLeftTower()
	{
		if (TowerLinker.Instance?.TowerMap[PlayerTeam.Team.Left] != null)
		{
			TowerLinker.Instance.TowerMap[PlayerTeam.Team.Left].ConstructPiece(RecipeBannerLinker.Instance.RecipeBannerMap[PlayerTeam.Team.Left].CurrentNeededItemType);
			Debug.Log("Constructed piece on left tower");
		}
		else
		{
			Debug.LogError("Left tower not found in WorldLinker");
		}
	}

	[Hotkey(KeyCode.V, "Construct right tower piece")]
	public void ConstructPieceOnRightTower()
	{
		if (TowerLinker.Instance.TowerMap[PlayerTeam.Team.Right] != null)
		{
			TowerLinker.Instance.TowerMap[PlayerTeam.Team.Right].ConstructPiece(RecipeBannerLinker.Instance.RecipeBannerMap[PlayerTeam.Team.Right].CurrentNeededItemType);
			Debug.Log("Constructed piece on right tower");
		}
		else
		{
			Debug.LogError("Right tower not found in WorldLinker");
		}
	}

    [Hotkey(KeyCode.B, "End Game instantly with high score")]
    public void EndGameInstantly()
    {
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

		yield return null; //not sure this is useful but might as well

        for (int i = 0; i < 8; i++)
            IncrementTimer();
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
	public void SetTimerToMax() => IncrementTimer(120f);

	[Hotkey(KeyCode.Period, "Set time scale to 0.1 or back to 1")]
	public void ToggleTimeScaleSlow() => ToggleTimeScale(.1f);
}
#endif
