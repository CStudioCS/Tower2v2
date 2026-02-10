using UnityEngine;
using UnityEngine.UI;
public abstract class IActionButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button buttonSerializeField;
    protected Button button => buttonSerializeField; 
    public abstract void Action();

    protected abstract void Movement();

    public abstract void OnClick();

}
