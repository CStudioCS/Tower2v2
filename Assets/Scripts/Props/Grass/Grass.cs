using UnityEngine;

public class Grass : MonoBehaviour
{
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Color RandomColor => colorGradient.Evaluate(Random.Range(0f, 1f));
    
    public void ApplyRandomColor() => spriteRenderer.color = RandomColor;
}