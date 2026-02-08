using UnityEngine;

public class ButtonQuitAction : ButtonActionWithMovement
{
    public void Action()
    {
        Application.Quit();
    }
}
