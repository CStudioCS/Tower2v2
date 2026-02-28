using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{

    [SerializeField] private Button[] buttons;

    public void SetButtonsInteractable(bool interactable = true)
    {
        foreach (Button button in buttons)
            button.interactable = interactable;
    }
}
