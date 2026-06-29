using Fusion;
using UnityEngine;

public class WorkbenchItemCatcher : MonoBehaviour
{
    [SerializeField] private Workbench workbench;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || !collision.gameObject.TryGetComponent(out Item item))
            return;

        NetworkObject netObj = item.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.HasStateAuthority)
            return;

        if (item.ItemType != Item.Type.WoodLog)
            return;

        if (item.State != Item.ItemState.Dropped)
            return;

        if (workbench.NetworkState != Workbench.State.Empty)
            return;

        workbench.PutWoodLog(item.originallyCollectedByTeam);
        netObj.Runner.Despawn(netObj);
    }
}
