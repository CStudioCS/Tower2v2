using UnityEngine;

public class TrashcanItemDetector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider != null && collider.gameObject.TryGetComponent<Item>(out Item item))
        {
            Destroy(collider.gameObject);
        }
    }
}
