using UnityEngine;

public class TowerItemCatcher : MonoBehaviour
{
    [SerializeField] private Tower tower;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider != null && collider.gameObject.TryGetComponent<Item>(out Item item))
        {
            if(tower.IsItemCorrect(item))
            {
                tower.ConstructPiece(item);
                Destroy(collider.gameObject);
            }
        }
    }
}
