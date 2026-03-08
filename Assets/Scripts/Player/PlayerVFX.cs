using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private VisualEffect grabItemSmokePoof;
    [SerializeField] private VisualEffect playerCollionSmokePoof;
    [SerializeField] private VisualEffect accelerationSmokePoof;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Vector2 leftAcceleratingParticulesPosition;
    [SerializeField] private Vector2 rightAcceleratingParticulesPosition;

    private void OnEnable()
    {
        player.GrabbedNewItem += OnGrabbedNewItem;
    }

    private void Update()
    {
        if (player.PlayerMovement.Accelerating)
            AcceleratingUpdate();
    }

    private void OnGrabbedNewItem()
    {
        grabItemSmokePoof.Play();
    }

    private void PlayCollideWithPlayerSmokePoof(Vector2 position)
    {
        playerCollionSmokePoof.transform.position = position;
        playerCollionSmokePoof.Play();
    }

    private void AcceleratingUpdate()
    {
        if (!spriteRenderer.flipX)
            accelerationSmokePoof.transform.localPosition = rightAcceleratingParticulesPosition;
        else
            accelerationSmokePoof.transform.localPosition = leftAcceleratingParticulesPosition;

        accelerationSmokePoof.Play();
    }

    private void OnDisable()
    {
        player.GrabbedNewItem -= OnGrabbedNewItem;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.transform.parent != null && collision.collider.transform.parent.TryGetComponent<Player>(out Player _))
            PlayCollideWithPlayerSmokePoof(collision.GetContact(0).point);
    }
}
