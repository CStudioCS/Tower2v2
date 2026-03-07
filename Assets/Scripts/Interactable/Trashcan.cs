using UnityEngine;

public class Trashcan : Interactable
{
    [SerializeField] private AudioSource audioSource;

    public void PlaySound()
    {
        audioSource.Play();
    }
    public override void Interact(Player player)
    {
        player.ConsumeCurrentItem();
    }

    public override float GetInteractionTime() => 0;

    public override bool CanInteract(Player player) => player.IsHolding && LevelManager.InGame;
}
