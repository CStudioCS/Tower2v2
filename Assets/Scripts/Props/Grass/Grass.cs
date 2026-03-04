using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Color RandomColor => colorGradient.Evaluate(Random.Range(0f, 1f));
    
    public void RandomizeColor() => spriteRenderer.color = RandomColor;
    public void RandomizeOrientation() => spriteRenderer.flipX = Random.Range(0, 2) == 1;
    public void SetDebugColor() => spriteRenderer.color = Color.blue;
}