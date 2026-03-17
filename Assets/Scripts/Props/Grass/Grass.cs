using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Color Color => spriteRenderer.color;
    
    private Color RandomColor => colorGradient.Evaluate(Random.Range(0f, 1f));
    public static readonly Color DebugColor = Color.blue;

    private void RandomizeColor() => spriteRenderer.color = RandomColor;
    private void RandomizeOrientation() => spriteRenderer.flipX = Random.Range(0, 2) == 1;

    public void Randomize()
    {
        RandomizeColor();
        RandomizeOrientation();
    }

    public void SetDebugColor() => spriteRenderer.color = DebugColor;
}