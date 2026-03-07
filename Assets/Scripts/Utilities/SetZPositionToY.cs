using UnityEngine;

/// <summary>
/// Setting the Z position the same as the Y position is useful in top down to use the built-in Z write of shader.
/// This way sprites can right their Y position to the depth buffer.
/// This can then be used in shaders, like in a silhouette shader for instance.
/// Can be marked as static for a small optimization.
/// </summary>
public class SetZPositionToY: MonoBehaviour
{
	[SerializeField] private bool isStatic = true;

	private void Start() => UpdatePosition();

	private void UpdatePosition()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
	}

	private void LateUpdate()
	{
		if (isStatic)
			return;
		UpdatePosition();
	}
}
