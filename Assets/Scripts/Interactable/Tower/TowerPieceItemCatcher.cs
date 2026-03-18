using UnityEngine;

public class TowerPieceItemCatcher : MonoBehaviour
{
	[SerializeField] private TowerPiece towerPiece;
	
	private void OnTriggerEnter2D(Collider2D collider)
	{
		towerPiece.Tower.TowerItemCatcher.Trigger(collider);
	}
}