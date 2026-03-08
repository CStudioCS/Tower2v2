using UnityEngine;

public class WorkbenchItemCatcher : MonoBehaviour
{
    [SerializeField] private Workbench workbench;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null || !collision.gameObject.TryGetComponent<Item>(out Item item))
            return;
        
        if (item.ItemType != Item.Type.WoodLog)
            return;

        if (item.State != Item.ItemState.Dropped)
            return;

        if (workbench.WorkbenchState != Workbench.State.Empty)
            return;


        workbench.TakeWood();
        Destroy(collision.gameObject);
    }
}
