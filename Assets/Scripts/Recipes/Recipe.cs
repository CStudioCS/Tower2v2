using UnityEngine;

public class Recipe: MonoBehaviour
{
    [SerializeField] private Resources.Type type;
    public Resources.Type Type => type;
}
