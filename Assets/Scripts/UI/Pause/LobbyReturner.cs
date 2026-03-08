using UnityEngine;

public class LobbyReturner : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenu;
    
    public void ReturnToLobby()
    {
        pauseMenu.Resume(fireEvent: false);
        LevelManager.Instance.ForceReturnToLobby();
    } 
}
