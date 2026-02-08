using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonPlayAction : ButtonActionWithMovement
{
    public override void Action()
    {
        SceneManager.LoadScene(0);
    }
}
