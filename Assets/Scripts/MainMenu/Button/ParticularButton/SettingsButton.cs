using UnityEngine;

public class SettingsButton : MovingButton
{
    [SerializeField] private ButtonSelectionManager settingsSelectionManager;

    public override void Action()
    {
        settingsSelectionManager.PauseSelection(false);
    }
}
