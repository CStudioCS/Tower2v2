using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMenuButton : ActionButton
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private string menuSceneName;
    [SerializeField] private GameObject returnMenuConfirmationGameObject;
    [SerializeField] private ButtonSelectionManager buttonSelectionMenu ;
    [SerializeField] private ButtonSelectionManager confirmationSelectionManager;

    public override void Action(){ 
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(menuSceneName); 
    }

    public override void OnClick()
    {
        returnMenuConfirmationGameObject.SetActive(true);
        buttonSelectionMenu.PauseSelection(true);
        confirmationSelectionManager.numOfButtons = 0;

    }

}
