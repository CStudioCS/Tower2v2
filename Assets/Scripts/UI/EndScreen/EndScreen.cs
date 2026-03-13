using LitMotion;
using System.Collections;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private float BigEndWaitTime = 1.5f;
    [SerializeField] private Vector2 dropdownOffset;

    [SerializeField] private BigEnd BigEnd;
    [SerializeField] private TowerCard TowerCard;
    [SerializeField] private StatsCard StatsCard;

    private void Start()
    {
        LevelManager.Instance.GameEnded += OnGameEnded;
    }

    private void OnGameEnded() => StartCoroutine(OnGameEndedCoroutine());

    private IEnumerator OnGameEndedCoroutine()
    {
        //animate this
        BigEnd.gameObject.SetActive(true);
        yield return BigEnd.Display();

        yield return new WaitForSeconds(BigEndWaitTime);

        TowerCard.gameObject.SetActive(true);
        yield return TowerCard.Dropdown();

        BigEnd.gameObject.SetActive(false);

        StatsCard.gameObject.SetActive(true);
        yield return StatsCard.Dropdown(TowerCard);

        //TowerCard.gameObject.SetActive(false); already done within StatsCard
        StatsCard.gameObject.SetActive(false);

        LevelManager.Instance.SetGameStateToLobby();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        LevelManager.Instance.GameEnded -= OnGameEnded;
    }
}
