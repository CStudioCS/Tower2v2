using UnityEngine;
using System.Collections.Generic;

public class Item : Interactable
{
    public enum Type { Straw, WoodLog, WoodPlank, Clay, Brick }
    
    [Header("Item")]
    [SerializeField] private Type itemType;
    public Type ItemType => itemType;
    public Player LastOwner;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D itemCollider;
    [SerializeField] private float ejectionSpeedMultiplier;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float ejectionSpeedVariance;
    [SerializeField] private float rotationSpeedVariance;
    [SerializeField] private float minimumEjectionSpeedRatio;
    [SerializeField] private float grabbingTime;

    public float GrabbingTime => grabbingTime;

    private void Awake()
    {
        itemCollider.enabled = false;
        LevelManager.Instance.GameEnded += Disappear;
    }

    public override bool CanInteract(Player player) => !player.IsHolding;

    public override void Interact(Player player)
    {
        player.GrabItem(this, true);
    }

    public override float GetInteractionTime() => 0;
    public void Immobilize()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.simulated = false;
        itemCollider.enabled = false;
    }

    public void Drop()
    {
        transform.SetParent(null);
        rb.simulated = true;
        itemCollider.enabled = true;

        float ejectionDeviation = Random.Range(1 - ejectionSpeedVariance, 1 + ejectionSpeedVariance);
        float rotationDeviation = Random.Range(1 - rotationSpeedVariance, 1 + rotationSpeedVariance);

        Vector2 lastSpeed = LastOwner.PlayerMovement.LastSpeed;
        Vector2 speedDirection =lastSpeed.normalized;
        Debug.Log(speedDirection);
        float ejectionSpeedRecalibration = ejectionSpeedMultiplier * Mathf.Clamp(Mathf.Abs(lastSpeed.magnitude), minimumEjectionSpeedRatio * LastOwner.PlayerMovement.MaxSpeed, LastOwner.PlayerMovement.MaxSpeed);//speed if not null else a percentage of max speed
        rb.linearVelocity = ejectionSpeedRecalibration * speedDirection * ejectionDeviation;
        rb.angularVelocity = (new List<int> { -1, 1 })[Random.Range(0, 2)] * rotationSpeed * rotationDeviation;
        LastOwner = null;
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
