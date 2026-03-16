using System;
using UnityEngine;

public class TowerHeightReachedActivator : MonoBehaviour
{
	[SerializeField] private Tower tower;
	[SerializeField] private GameObject gameObjectToActivate;
	[SerializeField] private int minimumHeight = 12;

	private void Start()
	{
		tower.PieceBuilt += OnPieceBuilt;
		LevelManager.Instance.GameAboutToStart += OnGameAboutToStart;
		Activate(false);
	}

	private void OnGameAboutToStart() => Activate(false);
	
	private void Activate(bool active = true) => gameObjectToActivate.SetActive(active);

	private void OnPieceBuilt()
	{
		if (tower.Height >= minimumHeight) Activate();
	}

	private void OnDisable()
	{
		tower.PieceBuilt -= OnPieceBuilt;
		LevelManager.Instance.GameAboutToStart -= OnGameAboutToStart;
	}
}