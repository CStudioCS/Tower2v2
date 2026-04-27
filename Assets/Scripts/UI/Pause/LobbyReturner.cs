using Fusion;
using UnityEngine;

public class LobbyReturner : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenu;
    
    public void ReturnToLobby()
    {
        pauseMenu.Resume(fireEvent: false);

        if (NetworkManager.Instance?.IsClient == true)
            LevelManager.Instance.ClientLeave();
        else
            LevelManager.Instance.ForceReturnToLobby();
    } 
}
