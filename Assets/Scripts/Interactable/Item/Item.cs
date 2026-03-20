using System;
using UnityEngine;
using System.Collections.Generic;
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
        LevelManager.Instance.GameEndedOrReturnedToLobby += Disappear;
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
        State = ItemState.Transitioning;
        player.GrabItem(this, true);
        Grabbed?.Invoke();
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
        transform.SetParent(null);
        rb.simulated = true;
        itemCollider.enabled = true;

        rb.linearVelocity = throwSpeed;
        velocitySqrMagnitudeForTowerItemCatcher = throwSpeed.sqrMagnitude;
        float rotationDeviation = Random.Range(1 - rotationSpeedVariance, 1 + rotationSpeedVariance);
        rb.angularVelocity = new List<int> { -1, 1 }[Random.Range(0, 2)] * rotationSpeed * rotationDeviation;

        trailRenderer.emitting = true;
        State = ItemState.Dropped;
        Dropped?.Invoke();
        SoundManager.instance.PlaySound("ItemDrop");
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
        LevelManager.Instance.GameEndedOrReturnedToLobby -= Disappear;
    }
}
