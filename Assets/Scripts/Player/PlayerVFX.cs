using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private VisualEffect grabItemSmokePoof;
    [SerializeField] private VisualEffect playerCollionSmokePoof;

    private void OnEnable()
    {
        player.GrabbedNewItem += OnGrabbedNewItem;
    }

    private void OnGrabbedNewItem()
    {
        grabItemSmokePoof.Play();
    }

    private void OnPlayerCollision(Vector2 position)
    {
        playerCollionSmokePoof.transform.position = position;
        playerCollionSmokePoof.Play();
    }

    private void OnDisable()
    {
        player.GrabbedNewItem -= OnGrabbedNewItem;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.transform.parent != null && collision.collider.transform.parent.TryGetComponent<Player>(out Player _))
            OnPlayerCollision(collision.GetContact(0).point);
    }
}
