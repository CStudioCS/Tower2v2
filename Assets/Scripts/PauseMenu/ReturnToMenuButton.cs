using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenuButton : ActionButton
{
    [SerializeField] private string menuSceneName;
    [SerializeField] private ConfirmationPopupContainer confirmationPopUpContainer;

    public override void Action(){ 
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(menuSceneName); 
    }

    public override void OnClick()
    {
        confirmationPopUpContainer.OpenPopUp();
    }
}
