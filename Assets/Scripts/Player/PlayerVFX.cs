using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private VisualEffect grabItemSmokePoof;
    [SerializeField] private VisualEffect playerCollionSmokePoof;
    [SerializeField] private VisualEffect accelerationSmokePoof;

    private int consecutiveAccelerationCount;
    [Tooltip("for X frame where we accelerate, we emit a particule")]
    [SerializeField] private int accelerationParticuleRate = 10;

    private void OnEnable()
    {
        player.GrabbedNewItem += OnGrabbedNewItem;
        playerMovement.Accelerating += OnAccelerating;
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

    private void OnAccelerating()
    {
        if(consecutiveAccelerationCount % accelerationParticuleRate == 0)
        {
            consecutiveAccelerationCount = 0;
            accelerationSmokePoof.Play();
        }

        consecutiveAccelerationCount++;
    }

    private void OnDisable()
    {
        player.GrabbedNewItem -= OnGrabbedNewItem;
        playerMovement.Accelerating -= OnAccelerating;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.transform.parent != null && collision.collider.transform.parent.TryGetComponent<Player>(out Player _))
            OnPlayerCollision(collision.GetContact(0).point);
    }
}
