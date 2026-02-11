using UnityEngine;
using System.Collections.Generic;
public class Item : Interactable
{
    public enum Type { Straw, WoodLog, WoodPlank, Clay, Brick }
    
    [Header("Item")]
    [SerializeField] private Type itemType;
    public Type ItemType => itemType;
    private Player lastOwner;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D itemCollider;
    [SerializeField] private float ejectionCoefficient;
    [SerializeField] private float rotationCoefficient;
    [SerializeField] private float ejectionCoefficientMaxDeviation;
    [SerializeField] private float rotationCoefficientMaxDeviation;
    [SerializeField] private float minimumEjectionSpeedRatio;

    private void Awake()
    {
        itemCollider.enabled = false;
    }

    protected override bool CanInteractPrimary(Player player) => !player.IsHolding;
    protected override void InteractPrimary(Player player)
    {
        Grab(player);
    }

    public void Grab(Player player)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0;
        rb.simulated = false;
        transform.rotation = Quaternion.identity;
        lastOwner = player;
        itemCollider.enabled = false;
        player.GrabItem(this);
        transform.SetParent(player.transform);
    }

    public void Drop()
    {
        transform.SetParent(null);
        rb.simulated = true;
        itemCollider.enabled = true;

        float ejectionDeviation = Random.Range(1 - ejectionCoefficientMaxDeviation, 1 + ejectionCoefficientMaxDeviation);
        float rotationDeviation = Random.Range(1 - rotationCoefficientMaxDeviation, 1 + rotationCoefficientMaxDeviation);

        Vector2 lastSpeed = lastOwner.PlayerMovement.LastSpeed;
        Vector2 speedDirection =lastSpeed.normalized;
        float ejectionSpeedRecalibration = Mathf.Clamp(Mathf.Abs(lastSpeed.magnitude), minimumEjectionSpeedRatio * lastOwner.PlayerMovement.MaxSpeed, lastOwner.PlayerMovement.MaxSpeed);//speed if not null else a percentage of max speed
        rb.linearVelocity = ejectionSpeedRecalibration * speedDirection * ejectionCoefficient * ejectionDeviation;
        rb.angularVelocity = (new List<int> { -1, 1 })[Random.Range(0, 2)] * rotationCoefficient * rotationDeviation;
        lastOwner = null;
    }
}
