using UnityEngine;
using UnityEngine.VFX;

public class PlayerVFX : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private VisualEffect vfx;

    private void OnEnable()
    {
        player.GrabbedNewItem += OnGrabbedNewItem;
    }

    private void OnGrabbedNewItem()
    {
        vfx.Play();
    }

    private void OnDisable()
    {
        player.GrabbedNewItem -= OnGrabbedNewItem;
    }
}
