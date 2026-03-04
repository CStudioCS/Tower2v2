using UnityEngine;

public class Trashcan : Interactable
{
    [SerializeField] private AudioSource audioSource;
    public override void Interact(Player player)
    {
        audioSource.Play();
        player.ConsumeCurrentItem();
    }

    public override float GetInteractionTime() => 0;

    public override bool CanInteract(Player player) => player.IsHolding;
}
