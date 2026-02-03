using UnityEngine;

public class Recipe : MonoBehaviour
{
    [SerializeField] private WorldResources.Type type;
    public WorldResources.Type Type => type;
}
