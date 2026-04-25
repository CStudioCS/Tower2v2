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

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void SetRiverVisualSpeed(float multiplier = 1f)
    {
        if (propBlock == null || riverRenderer == null) 
			return;

        riverRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(SpeedString, Speed * multiplier);
        riverRenderer.SetPropertyBlock(propBlock);
    }

	private void OnGameStarted() => SetRiverVisualSpeed(1f);
	private void OnGameEnded() => SetRiverVisualSpeed(0f);

    private void OnReturnedToLobby()
    {
        SetRiverVisualSpeed(0f);
        if (initialPosition != null)
            transform.localPosition = (Vector2)initialPosition;
    }

    private void LateUpdate()
    {
        if (!LevelManager.InGame || initialPosition == null)
            return;

        float elapsedTime = LevelManager.Instance.LevelTimer;
        float traveledDistance = elapsedTime * Speed;

        float newX = initialPosition.Value.x + (traveledDistance % WorldSize);

        if (newX >= semiWorldSize)
            newX -= WorldSize;

        transform.localPosition = new Vector2(newX, initialPosition.Value.y);
    }

	private void OnDisable()
	{
		LevelManager.GameStarted -= OnGameStarted;
		LevelManager.GameEnded -= OnGameEnded;
		LevelManager.ReturnedToLobby -= OnReturnedToLobby;
	}
}
