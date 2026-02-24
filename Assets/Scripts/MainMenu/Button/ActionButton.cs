using UnityEngine;
using UnityEngine.UI;

public abstract class ActionButton : MonoBehaviour
{
    [SerializeField] private Button button;

    protected Button Button => button; 

    public abstract void Action();

    public abstract void OnClick();
}
