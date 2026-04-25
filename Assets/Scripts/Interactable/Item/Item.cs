using Fusion.Addons.Physics;
using System;
using Fusion;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;
using Random = UnityEngine.Random;

public class Item : Interactable
{
    private static readonly int SilhouetteColorString = Shader.PropertyToID("_SilhouetteColor");

    public enum Type { Straw, WoodLog, WoodPlank, Clay, Brick }

    [Header("Item")]
    [SerializeField] private Type itemType;
    public Type ItemType => itemType;
    [SerializeField] private Color silhouetteColor = Color.black;
    public Player LastOwner { get; set; }
    [SerializeField] private Rigidbody2D rb;
    public float velocitySqrMagnitudeForTowerItemCatcher;
    [SerializeField] private Collider2D itemCollider;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private GameObject graphics;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float rotationSpeedVariance;
    [SerializeField] private float minimumEjectionSpeedRatio;
    [SerializeField] private float grabbingTime;

    [Header("Network")]
    [SerializeField] private NetworkItem networkItem;
    [SerializeField] private NetworkRigidbody2D netRb;
    public NetworkItem NetworkItem => networkItem;
    public enum ItemState { Held, Dropped, Transitioning };
    public ItemState State { get; set; }

    [HideInInspector] public PlayerTeam.Team originallyCollectedByTeam;
    public float GrabbingTime => grabbingTime;

    public event Action Grabbed;
    public event Action Dropped;

    protected override void Awake()
    {
        base.Awake(); // Initialize highlight system
        itemCollider.enabled = false;
        State = ItemState.Dropped;
        LevelManager.GameEnded += Disappear;
        trailRenderer.emitting = false;
        SetSilhouetteColor(silhouetteColor);
    }

    private void SetSilhouetteColor(Color color)
    {
        MaterialPropertyBlock propBlock = new();
        spriteRenderers[0].GetPropertyBlock(propBlock);
        propBlock.SetColor(SilhouetteColorString, color);
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            spriteRenderer.SetPropertyBlock(propBlock);   
        }
    }

    public override bool CanInteract(Player player) => !player.IsHolding && State == ItemState.Dropped && LevelManager.InGame;
    public override void Interact(Player player)
    {
        if (player.HasStateAuthority)
        {
            networkItem.Grab(player, true);
            Grabbed?.Invoke();
        }
    }

    public override float GetInteractionTime() => 0;
    public void Immobilize()
    {
        rb.linearVelocity = Vector2.zero;
        velocitySqrMagnitudeForTowerItemCatcher = 0;
        rb.angularVelocity = 0;
        rb.simulated = false;
        itemCollider.enabled = false;
        trailRenderer.emitting = false;
    }

    public void Drop() => Drop(Vector2.zero);
 
    public void Drop(Vector2 throwSpeed)
    {
        if (!networkItem.HasStateAuthority) return;

        networkItem.Drop();

        transform.SetParent(null);
        rb.simulated = true;
        itemCollider.enabled = true;
        netRb.enabled = true;
        netRb.Teleport(transform.position, transform.rotation);

        rb.linearVelocity = throwSpeed;
        velocitySqrMagnitudeForTowerItemCatcher = throwSpeed.sqrMagnitude;
        float rotationDeviation = Random.Range(1 - rotationSpeedVariance, 1 + rotationSpeedVariance);
        rb.angularVelocity = new List<int> { -1, 1 }[Random.Range(0, 2)] * rotationSpeed * rotationDeviation;
    }

    public void ApplyNetworkState(ItemState newState, Player newOwner, PlayerTeam.Team originalTeam)
    {
        if (newState == ItemState.Held && newOwner != null && (State != ItemState.Held || LastOwner != newOwner))
        {
            State = ItemState.Transitioning;

            if (LastOwner != null && LastOwner != newOwner && LastOwner.HeldItem == this)
                LastOwner.HeldItem = null;

            LastOwner = newOwner;
            originallyCollectedByTeam = originalTeam;
            Immobilize();
            netRb.enabled = false;

            // GrabItem function in Player.cs sets item.State = Held at the end
            newOwner.GrabItem(this, networkItem.SyncAnimateGrab);
        }
        else if (newState == ItemState.Dropped)
        {
            State = ItemState.Dropped;
            transform.SetParent(null);
            rb.simulated = true;
            netRb.enabled = true;
            itemCollider.enabled = true;

            trailRenderer.Clear();
            trailRenderer.emitting = true;

            if (LastOwner != null && LastOwner.HeldItem == this)
                LastOwner.HeldItem = null;

            Dropped?.Invoke();
            SoundManager.instance.PlaySound("ItemDrop");
        }
    }

    public void SetFlipX(bool flipped) => graphics.transform.localScale = new Vector2(flipped ? -1f : 1f, 1f);

    private void FixedUpdate()
    {
        velocitySqrMagnitudeForTowerItemCatcher = Mathf.Min(rb.linearVelocity.sqrMagnitude, velocitySqrMagnitudeForTowerItemCatcher);
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        LevelManager.GameEnded -= Disappear;
    }
}
