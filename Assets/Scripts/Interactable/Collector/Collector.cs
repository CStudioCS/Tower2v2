using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// This is everything from wood collectors, cement collectors and dirt collectors. When interacted with, it makes a player hold the itemPrefab gameobject
/// corresponding to the given Item held item (maybe change it with a dictionnary that can match them up ?)
/// </summary>
public class Collector : Interactable
{
    [SerializeField] private float interactionTime;
    [Header("Collector")]
    [SerializeField] private Item itemPrefab;

    [SerializeField] private AudioSource audioSource;

    public override bool CanInteract(Player player) => !player.IsHolding;

    public override void Interact(Player player)
    {
        player.GrabNewItem(itemPrefab);
    }

    public override float GetInteractionTime() => interactionTime;

    private void Update()
    {
        if (IsAlreadyInteractedWith && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (!IsAlreadyInteractedWith && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
