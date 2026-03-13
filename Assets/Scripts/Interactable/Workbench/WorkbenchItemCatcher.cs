using UnityEngine;

public class WorkbenchItemCatcher : MonoBehaviour
{
    [SerializeField] private Workbench workbench;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || !collision.gameObject.TryGetComponent(out Item item))
            return;
        
        if (item.ItemType != Item.Type.WoodLog)
            return;

        if (item.State != Item.ItemState.Dropped)
            return;

        if (workbench.WorkbenchState != Workbench.State.Empty)
            return;

        workbench.PutWoodLog();
        Destroy(collision.gameObject);
    }
}
