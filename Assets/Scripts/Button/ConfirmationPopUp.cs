using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfirmationPopUp : MonoBehaviour
{

    [SerializeField] private Button[] boutonsMenu;
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject selectedButtonWhenOpenPopUp;
    [SerializeField] private GameObject selectedButtonWhenQuitPopUp;

    public void OpenPopUp()
    {
        foreach (var b in boutonsMenu)
            b.interactable = false;

        popup.SetActive(true);
        EventSystem.current.SetSelectedGameObject(selectedButtonWhenOpenPopUp);
    }

    public void ClosePopUp()
    {
        foreach (var b in boutonsMenu)
            b.interactable = true;

        popup.SetActive(false);
        EventSystem.current.SetSelectedGameObject(selectedButtonWhenQuitPopUp);
    }
}
