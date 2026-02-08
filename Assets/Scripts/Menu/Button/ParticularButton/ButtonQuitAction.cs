using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class ButtonQuitAction : ButtonActionWithMovement
{
    public override void  Action()
    {
        #if UNITY_EDITOR
            Debug.Log("Stop Play Mode in Editor");
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }
}
