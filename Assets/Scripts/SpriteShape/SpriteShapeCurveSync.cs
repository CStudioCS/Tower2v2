#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class SpriteShapeCurveSync : MonoBehaviour
{
    [SerializeField] private SpriteShapeController parentShape;
    [SerializeField] private SpriteShapeController childShape;
    [SerializeField] private float offsetDistance;
    [SerializeField] private bool syncInRealTime = true;

    private void OnValidate()
    {
        if (parentShape == null)
            parentShape = GetComponent<SpriteShapeController>();
    }

    private void Update()
    {
        if (syncInRealTime && parentShape != null && childShape != null)
            SyncChildToParent();
    }

    [ContextMenu("Sync Child to Parent")]
    public void SyncChildToParent()
    {
        if (parentShape == null || childShape == null)
        {
            Debug.LogWarning("Parent or Child SpriteShapeController not assigned!");
            return;
        }

        Spline parentSpline = parentShape.spline;
        Spline childSpline  = childShape.spline;

        int pointCount = parentSpline.GetPointCount();
        if (pointCount == 0) return;

        // ── Snapshot everything from the parent ──────────────────────────────
        var positions     = new Vector3[pointCount];
        var tangentModes  = new ShapeTangentMode[pointCount];
        var rightTangents = new Vector3[pointCount];
        var leftTangents  = new Vector3[pointCount];
        var heights       = new float[pointCount];
        var spriteIndices = new int[pointCount];

        // Convert all positions: parent-local → world → child-local
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 worldPos = parentShape.transform.TransformPoint(parentSpline.GetPosition(i));
            positions[i] = childShape.transform.InverseTransformPoint(worldPos);
        }

        // Build offset positions using child-local tangents
        var offsetPositions = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 offsetDir = Vector3.up;
            if (pointCount > 1)
            {
                Vector3 prev = positions[i > 0 ? i - 1 : i];
                Vector3 next = positions[i < pointCount - 1 ? i + 1 : i];

                Vector3 tangent;
                if (i == 0)
                    tangent = (next - positions[i]).normalized;
                else if (i == pointCount - 1)
                    tangent = (positions[i] - prev).normalized;
                else
                    tangent = (next - prev).normalized; // central difference — smoother

                offsetDir = new Vector3(-tangent.y, tangent.x, 0f).normalized;
            }

            offsetPositions[i] = positions[i] + offsetDir * offsetDistance;
        }

        // Snapshot tangent vectors: parent-local direction → world → child-local direction
        for (int i = 0; i < pointCount; i++)
        {
            tangentModes[i] = parentSpline.GetTangentMode(i);

            Vector3 worldRight = parentShape.transform.TransformDirection(parentSpline.GetRightTangent(i));
            Vector3 worldLeft  = parentShape.transform.TransformDirection(parentSpline.GetLeftTangent(i));

            rightTangents[i] = childShape.transform.InverseTransformDirection(worldRight);
            leftTangents[i]  = childShape.transform.InverseTransformDirection(worldLeft);

            heights[i]       = parentSpline.GetHeight(i);
            spriteIndices[i] = parentSpline.GetSpriteIndex(i);
        }

        // ── Rebuild child spline ─────────────────────────────────────────────
        childSpline.Clear();

        // Pass 1 — insert points
        for (int i = 0; i < pointCount; i++)
            childSpline.InsertPointAt(i, offsetPositions[i]);

        // Pass 2 — set tangent mode BEFORE writing tangent values
        for (int i = 0; i < pointCount; i++)
        {
            childSpline.SetTangentMode(i, tangentModes[i]);
            childSpline.SetRightTangent(i, rightTangents[i]);
            childSpline.SetLeftTangent(i, leftTangents[i]);
            childSpline.SetHeight(i, heights[i]);
            childSpline.SetSpriteIndex(i, spriteIndices[i]);
        }

        childSpline.isOpenEnded = parentSpline.isOpenEnded;

        childShape.RefreshSpriteShape();
    }
}

[CustomEditor(typeof(SpriteShapeCurveSync))]
public class SpriteShapeCurveSyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpriteShapeCurveSync sync = (SpriteShapeCurveSync)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Sync Child to Parent"))
            sync.SyncChildToParent();
    }
}
#endif