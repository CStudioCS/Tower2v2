using UnityEngine;

public class PlayerAimArrow: MonoBehaviour
{
	[SerializeField] private Player player;
	[SerializeField] private GameObject graphics;
	[SerializeField] private SpriteRenderer colorizedSpriteRenderer;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private float minArrowLength = 1.8f;
	[SerializeField] private float maxArrowLength = 3.5f;
	private float ArrowLength => (maxArrowLength - minArrowLength) * player.SyncThrowSpeedRatio + minArrowLength;
	
	private void OnEnable()
	{
		player.PlayerTeam.TeamChanged += OnTeamChanged;
        player.AvatarSpawned += UpdateColor;
        player.StartedAimingLockedIn += OnStartedAimingLockedIn;
        player.StoppedAiming += OnStoppedAiming;

        ShowGraphics(false);
	}

	private void OnStartedAimingLockedIn()
	{
		UpdateArrowLength();
		ShowGraphics();
	}

	private void OnStoppedAiming() => ShowGraphics(false);
	
	private void ShowGraphics(bool show = true) => graphics.SetActive(show);

	private void Update()
	{
		if (player?.Object?.IsValid != true)
            return;

        bool isAiming = player.CurrentAimingState == Player.AimingState.AimingLockedIn;

        if (graphics.activeSelf != isAiming)
            ShowGraphics(isAiming);

        if (!isAiming)
            return;

        transform.right = player.SyncThrowDirection;
        UpdateArrowLength();
	}

	private void UpdateArrowLength()
	{
        colorizedSpriteRenderer.size = new Vector2(ArrowLength, colorizedSpriteRenderer.size.y);
        spriteRenderer.size = new Vector2(ArrowLength, spriteRenderer.size.y);
    }

	private void OnTeamChanged() => UpdateColor();

	private void UpdateColor()
	{
		colorizedSpriteRenderer.color = PlayerTeam.TeamColors[player.PlayerTeam.CurrentTeam];
	}

	private void OnDisable()
	{
		player.PlayerTeam.TeamChanged -= OnTeamChanged;
        player.AvatarSpawned -= UpdateColor;
		player.StartedAimingLockedIn -= OnStartedAimingLockedIn;
		player.StoppedAiming -= OnStoppedAiming;
    }
}
