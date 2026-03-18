using System;
using UnityEngine;

public class TowerHeightReachedActivator : MonoBehaviour
{
	[SerializeField] private Tower tower;
	[SerializeField] private int minimumHeight = 12;

	public event Action HeightReached;
	
	private void Start()
	{
		tower.PieceBuilt += OnPieceBuilt;
	}

	private void OnPieceBuilt()
	{
		if (tower.Height >= minimumHeight) HeightReached?.Invoke();
	}

	private void OnDisable()
	{
		tower.PieceBuilt -= OnPieceBuilt;
	}
}