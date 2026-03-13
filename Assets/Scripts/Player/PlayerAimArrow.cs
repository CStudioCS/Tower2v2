using UnityEngine;

public class PlayerAimArrow: MonoBehaviour
{
	[SerializeField] private Player player;
	[SerializeField] private GameObject graphics;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer outlineSpriteRenderer;
	[SerializeField] private float minArrowLength = 1.8f;
	[SerializeField] private float maxArrowLength = 3.5f;
	private float ArrowLength => (maxArrowLength - minArrowLength) * player.throwSpeedRatio + minArrowLength;
	
	private void OnEnable()
	{
		player.StartedAimingLockedIn += OnStartedAimingLockedIn;
		player.StoppedAiming += OnStoppedAiming;
		player.PlayerTeam.TeamChanged += OnTeamChanged;
		ShowGraphics(false);
		UpdateColor();
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
		if (player.CurrentAimingState != Player.AimingState.AimingLockedIn)
			return;

		transform.right = player.ThrowDirection;
		UpdateArrowLength();
	}

	private void UpdateArrowLength()
	{
		UpdateArrowLength(spriteRenderer);
		UpdateArrowLength(outlineSpriteRenderer);
	}
	private void UpdateArrowLength(SpriteRenderer s) => s.size = new Vector2(ArrowLength, s.size.y);

	private void OnTeamChanged() => UpdateColor();

	private void UpdateColor()
	{
		spriteRenderer.color = PlayerTeam.TeamColors[player.PlayerTeam.CurrentTeam];
	}

	private void OnDisable()
	{
		player.StartedAimingLockedIn -= OnStartedAimingLockedIn;
		player.StoppedAiming -= OnStoppedAiming;
		player.PlayerTeam.TeamChanged -= OnTeamChanged;
	}
}
