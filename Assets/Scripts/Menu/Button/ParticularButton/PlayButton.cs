using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MovingButton
{
    [SerializeField] private string loadedSceneName;
    public override void Action()
    {
        SceneManager.LoadScene(loadedSceneName);
    }
}
