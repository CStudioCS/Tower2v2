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
    [SerializeField] private Collider2D itemCollider;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private float ejectionSpeedMultiplier;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float ejectionSpeedVariance;
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
        LevelManager.Instance.GameEnded += Disappear;
        trailRenderer.emitting = false;
        SetSilhouetteColor(silhouetteColor);
    }

    private void SetSilhouetteColor(Color color)
    {
        MaterialPropertyBlock propBlock = new();
        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(SilhouetteColorString, color);
        spriteRenderer.SetPropertyBlock(propBlock);
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

        float ejectionDeviation = Random.Range(1 - ejectionSpeedVariance, 1 + ejectionSpeedVariance);
        float rotationDeviation = Random.Range(1 - rotationSpeedVariance, 1 + rotationSpeedVariance);

        Vector2 throwDirection = throwSpeed.normalized;

        float ejectionSpeedRecalibration = ejectionSpeedMultiplier * Mathf.Clamp(throwSpeed.magnitude, minimumEjectionSpeedRatio * LastOwner.PlayerMovement.MaxSpeed, LastOwner.PlayerMovement.MaxSpeed);
        rb.linearVelocity = throwDirection * (ejectionSpeedRecalibration * ejectionDeviation);
        rb.angularVelocity = new List<int> { -1, 1 }[Random.Range(0, 2)] * rotationSpeed * rotationDeviation;

        trailRenderer.emitting = true;
        State = ItemState.Dropped;
        Dropped?.Invoke();
        SoundManager.instance.PlaySound("ItemDrop");
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        LevelManager.Instance.GameEnded -= Disappear;
    }
}
