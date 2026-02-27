using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{

    [SerializeField] private Button[] boutonsMenu;

    public void DesactivButton()
    {
        foreach (var b in boutonsMenu)
            b.interactable = false;
    }
    public void ActivButton()
    {
        foreach (var b in boutonsMenu)
            b.interactable = true;
    }
}
