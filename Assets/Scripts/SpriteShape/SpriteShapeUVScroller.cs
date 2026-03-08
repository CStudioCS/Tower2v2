using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "SpriteShapeUVScroller", menuName = "ScriptableObjects/SpriteShapeUVScroller", order = 1)]
public class SpriteShapeUVScroller : SpriteShapeGeometryModifier
{
	public float scrollSpeed = 1.0f;

	public override JobHandle MakeModifierJob(JobHandle generator, SpriteShapeController spriteShapeController, NativeArray<ushort> indices, NativeSlice<Vector3> positions, NativeSlice<Vector2> texCoords, NativeSlice<Vector4> tangents, NativeArray<SpriteShapeSegment> segments, NativeArray<float2> colliderData)
	{
		return new UVAnimatorJob { uvs = texCoords, offset = Time.time * scrollSpeed % 1.0f }.Schedule(generator);
	}
	
	struct UVAnimatorJob : IJob
	{
		public NativeSlice<Vector2> uvs;
		public float offset;

		public void Execute()
		{
			for (int i = 0; i < uvs.Length; ++i)
			{
				Vector2 uv = uvs[i];
				uv.x = (uv.x + offset) % 1.0f;
				uvs[i] = uv;
			}
		}
	}
}
