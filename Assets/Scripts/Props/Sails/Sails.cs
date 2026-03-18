using UnityEngine;

public class Sails : MonoBehaviour
{
    private static readonly int Visible = Animator.StringToHash("Visible");
    [SerializeField] private TowerHeightReachedActivator towerHeightReachedActivator;
    [SerializeField] private Animator animator;

    private void Start()
    {
        towerHeightReachedActivator.HeightReached += OnHeightReached;
        LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
    }

    private void OnHeightReached()
    {
        Animate();
        SoundManager.instance.PlaySound("Sails");
    }

    private void Animate(bool on = true) => animator.SetBool(Visible, on);

    private void OnGameAboutToStart() => Animate(false);

    private void OnDisable()
    {
        towerHeightReachedActivator.HeightReached -= OnHeightReached;
        LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
    }
}
