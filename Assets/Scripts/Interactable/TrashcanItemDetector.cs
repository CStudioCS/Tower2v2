using UnityEngine;

public class TrashcanItemDetector : MonoBehaviour
{
    [SerializeField] Trashcan trashcan;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider != null && collider.gameObject.TryGetComponent<Item>(out Item item))
        {
            SoundManager.instance.PlaySound("Trashcan");
            Destroy(collider.gameObject);
        }
    }
}
