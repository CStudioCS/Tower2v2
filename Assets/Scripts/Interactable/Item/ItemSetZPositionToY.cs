using UnityEngine;

public class ItemSetZPositionToY: MonoBehaviour
{
	[SerializeField] private Item item;
	[SerializeField] private float droppedZOffset = .5f;
	private float ZOffset => item.State == Item.ItemState.Dropped ? -droppedZOffset : 0;

	private void OnEnable()
	{
		item.Grabbed += OnGrab;
		item.Dropped += OnDrop;
	}

	private void OnGrab()
	{
		transform.localPosition = new Vector3(item.transform.localPosition.x, item.transform.localPosition.y, 0);
	}

	private void OnDrop()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y + ZOffset);
	}

	private void LateUpdate()
	{
		if (item.State == Item.ItemState.Dropped)
			UpdatePosition();
	}
}
