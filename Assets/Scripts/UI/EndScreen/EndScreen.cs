using LitMotion;
using System.Collections;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private float BigEndWaitTime = 1.5f;
    [SerializeField] private Vector2 dropdownOffset;

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

        yield return new WaitForSeconds(BigEndWaitTime);

        TowerCard.gameObject.SetActive(true);
        yield return TowerCard.Dropdown();

        StatsCard.gameObject.SetActive(true);
        yield return StatsCard.Dropdown(TowerCard);


        //The player quits using the UI so idk ??????
        BigEndText.SetActive(false);
        //TowerCard.gameObject.SetActive(false); already done withing StatsCard
        StatsCard.gameObject.SetActive(false);

        LevelManager.Instance.SetGameStateToLobby();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        LevelManager.Instance.GameEnded -= GameEnded;
    }
}
