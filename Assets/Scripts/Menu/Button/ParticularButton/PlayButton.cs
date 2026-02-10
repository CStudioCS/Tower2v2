using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MovingButton
{
    public override void Action()
    {
        SceneManager.LoadScene(0);
    }
}
