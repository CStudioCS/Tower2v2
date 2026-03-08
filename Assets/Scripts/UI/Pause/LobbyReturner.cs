using UnityEngine;

public class LobbyReturner : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenu;
    
    public void ReturnToLobby()
    {
        pauseMenu.Resume();
        LevelManager.Instance.ReturnToLobby();
    } 
}
