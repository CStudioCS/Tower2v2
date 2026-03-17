using UnityEngine;

public class RecipeMovableBanner : MonoBehaviour
{
    private static readonly int Visible = Animator.StringToHash("Visible");
    [SerializeField] private Animator animator;
    
    private void Start()
    {
        LevelManager.Instance.GameStarted += OnGameStarted;
        LevelManager.Instance.GameEndedOrReturnedToLobby += OnGameEndedOrReturnedToLobby;
    }
    
    private void AnimateShow(bool visible = true) => animator.SetBool(Visible, visible);

    private void OnGameStarted() => AnimateShow();

    private void OnGameEndedOrReturnedToLobby() => AnimateShow(false);
    
    private void OnDisable()
    {
        LevelManager.Instance.GameStarted -= OnGameStarted;
        LevelManager.Instance.GameEndedOrReturnedToLobby -= OnGameEndedOrReturnedToLobby;
    }
}
