using UnityEngine;

public class RecipeSlot : MonoBehaviour
{
    public Vector2 RecipePosition => transform.localPosition;
    public float RecipeScale => transform.localScale.x;
}