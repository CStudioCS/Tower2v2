using UnityEngine;

public class TrashcanItemDetector : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision != null)
        {
            if(collision.gameObject.TryGetComponent<Item>(out Item item))
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
