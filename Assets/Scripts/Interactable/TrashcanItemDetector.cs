using Fusion;
using UnityEngine;

public class TrashcanItemDetector : MonoBehaviour
{
    [SerializeField] Trashcan trashcan;
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == null || !collider.gameObject.TryGetComponent(out Item item))
            return;

        SoundManager.instance.PlaySound("Trashcan");

        NetworkObject netObj = item.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasStateAuthority)
            return;

        netObj.Runner.Despawn(netObj);
    }
}
