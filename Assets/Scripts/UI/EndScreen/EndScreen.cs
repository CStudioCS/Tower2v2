using System.Collections;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject BigEndText;
    [SerializeField] private TowerCard TowerCard;
    [SerializeField] private StatsCard StatsCard;

    private void Start()
    {
        LevelManager.Instance.GameEnded += GameEnded;
    }

    private void GameEnded()
    {
        StartCoroutine(OnGameEndedCoroutine());
    }

    private IEnumerator OnGameEndedCoroutine()
    {
        //animate this
        BigEndText.SetActive(true);

        yield return new WaitForSeconds(2);

        TowerCard.gameObject.SetActive(true);
        yield return TowerCard.Dropdown();

        StatsCard.gameObject.SetActive(true);
        yield return StatsCard.Dropdown();

        //The player quits using the UI so idk ??????
        BigEndText.SetActive(false);
        TowerCard.gameObject.SetActive(false);
        StatsCard.gameObject.SetActive(false);

        LevelManager.Instance.SetGameStateToLobby();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        LevelManager.Instance.GameEnded -= GameEnded;
    }
}
