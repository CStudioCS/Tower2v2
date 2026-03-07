using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(SpriteShapeController))]
public class SpriteShapeUVScrollerRefresher : MonoBehaviour
{
	[SerializeField] private SpriteShapeController controller;

	void Update()
	{
		controller.RefreshSpriteShape();
	}
}
