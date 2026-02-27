using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class SettingsButton : MovingButton
{
    [SerializeField] private Button quitSettingsButton;
    public override void Action()
    {
        quitSettingsButton.interactable = true;
        EventSystem.current.SetSelectedGameObject(quitSettingsButton.gameObject);
    }
}
