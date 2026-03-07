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

    [SerializeField] private string soundName;
    private int soundIndex;

    public override bool CanInteract(Player player) => !player.IsHolding && LevelManager.InGame;

    public override void Interact(Player player)
    {
        player.GrabNewItem(itemPrefab);
        player.PlayerStats.OnCollectedItem(itemPrefab.ItemType);
    }

    public override float GetInteractionTime() => interactionTime;

    private void Update()
    {
        if (IsAlreadyInteractedWith && soundIndex == -1)
        {
            soundIndex = SoundManager.instance.PlaySound(soundName);
        }

        if (!IsAlreadyInteractedWith && soundIndex != -1)
        {
            SoundManager.instance.StopSound(soundIndex);
            soundIndex = -1;
        }
    }
}
