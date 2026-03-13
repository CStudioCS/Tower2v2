using UnityEngine;

public class PlayerAimArrow: MonoBehaviour
{
	[SerializeField] private Player player;
	[SerializeField] private GameObject graphics;
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private float minArrowLength = 1.8f;
	[SerializeField] private float maxArrowLength = 3.5f;
	private float ArrowLength => (maxArrowLength - minArrowLength) * player.throwSpeedRatio + minArrowLength;
	
	private void OnEnable()
	{
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
		if (player.CurrentAimingState != Player.AimingState.AimingLockedIn)
			return;

		transform.right = player.ThrowDirection;
		UpdateArrowLength();
	}
	
	private void UpdateArrowLength() => spriteRenderer.size = new Vector2(ArrowLength, spriteRenderer.size.y);

	private void OnDisable()
	{
		player.StartedAimingLockedIn -= OnStartedAimingLockedIn;
		player.StoppedAiming -= OnStoppedAiming;
	}
}
