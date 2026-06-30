using Fusion;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFX : NetworkBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private VisualEffect grabItemSmokePoof;
    [SerializeField] private VisualEffect playerCollionSmokePoof;
    [SerializeField] private VisualEffect walkSmokePoof;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private float distanceBetweenPoofs = 1f;
    private float accumulatedDistance;

    private void OnEnable()
    {
        player.GrabbedNewItem += OnGrabbedNewItem;
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

    public override void Render()
    {
        accumulatedDistance += player.PlayerMovement.SyncVelocity.magnitude * Time.deltaTime;

        if (accumulatedDistance >= distanceBetweenPoofs)
        {
            walkSmokePoof.Play();
            accumulatedDistance -= distanceBetweenPoofs;
        }
    }

    private void OnDisable()
    {
        player.GrabbedNewItem -= OnGrabbedNewItem;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.magnitude < 4f || Runner.IsResimulation) return;

        if (collision.collider.transform.parent != null && collision.collider.transform.parent.TryGetComponent<Player>(out Player _))
            PlayCollideWithPlayerSmokePoof(collision.GetContact(0).point);
    }
}
