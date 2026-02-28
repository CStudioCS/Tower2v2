using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConfirmationPopupContainer : MonoBehaviour
{

    [SerializeField] private ButtonManager buttonManager;
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject selectedButtonWhenOpenPopUp;
    [SerializeField] private GameObject selectedButtonWhenQuitPopUp;

    public void OpenPopUp()
    {
        buttonManager.SetButtonsInteractable(false);
        popup.SetActive(true);
        EventSystem.current.SetSelectedGameObject(selectedButtonWhenOpenPopUp);
    }

    public void ClosePopUp()
    {

        buttonManager.SetButtonsInteractable(true);

        popup.SetActive(false);
        EventSystem.current.SetSelectedGameObject(selectedButtonWhenQuitPopUp);
    }
}
