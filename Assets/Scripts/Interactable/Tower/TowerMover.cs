using UnityEngine;

public class TowerMover : MonoBehaviour
{
	private static readonly int SpeedString = Shader.PropertyToID("_Speed");
	[SerializeField] private int loopsInAGame = 6;
	[SerializeField] private float semiWorldSize = 12f;
	private float WorldSize => 2 * semiWorldSize;
	private float? speed;
	private float Speed
	{
		get
		{
			speed ??= loopsInAGame * WorldSize / LevelManager.Instance.TimerLimit;
			return speed.Value;
		}
	}
	private Vector2 Velocity => Speed * Vector2.right;
	[SerializeField] private Rigidbody2D rb;
	private Vector2? initialPosition;
	[SerializeField] private Renderer riverRenderer;
	private MaterialPropertyBlock propBlock;

	private void Awake()
	{
		initialPosition ??= transform.localPosition;
		if (propBlock != null) 
			return;
		if (riverRenderer == null)
			return;
		propBlock = new MaterialPropertyBlock();
	}

	private void Start()
	{
		LevelManager.GameStarted += OnGameStarted;
		LevelManager.GameEnded += OnGameEnded;
		LevelManager.ReturnedToLobby += OnReturnedToLobby;
	}

	private void SetVelocity(float multiplier = 1f)
	{
		rb.linearVelocity = Velocity * multiplier;
		if (propBlock == null) 
			return;
		if (riverRenderer == null)
			return;
		riverRenderer.GetPropertyBlock(propBlock);
		propBlock.SetFloat(SpeedString, Speed * multiplier);
		riverRenderer.SetPropertyBlock(propBlock);
	}

	private void OnGameStarted()
	{
		ResetPosition();
		SetVelocity(1f);
	}

	private void OnGameEnded() => SetVelocity(0f);

	private void OnReturnedToLobby() => ResetPositionAndVelocity();

	private void ResetPositionAndVelocity()
	{
		ResetPosition();
		SetVelocity(0f);
	}

	private void ResetPosition()
	{
		if (initialPosition != null)
			transform.localPosition = (Vector2)initialPosition;
	}

	private void LateUpdate()
	{
		if (!LevelManager.InGame)
			return;
		if (transform.localPosition.x >= semiWorldSize)
			transform.localPosition = new Vector2(transform.localPosition.x - WorldSize, transform.localPosition.y);
	}

	private void OnDisable()
	{
		LevelManager.GameStarted -= OnGameStarted;
		LevelManager.GameEnded -= OnGameEnded;
		LevelManager.ReturnedToLobby -= OnReturnedToLobby;
	}
}
