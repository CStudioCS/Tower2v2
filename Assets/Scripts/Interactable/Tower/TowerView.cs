using System;
using UnityEngine;
using UnityEngine.VFX;

public class TowerView : MonoBehaviour
{
    [SerializeField] private Tower tower;
    [SerializeField] private VisualEffect vfx;

    private void OnEnable()
    {
        tower.PieceBuilt += OnPieceBuilt;
        
        // This is weird, but I figured it preloads the vfx to initialize correctly.
        vfx.SendEvent("OnPlay");
        vfx.Stop();
    }

    private void OnPieceBuilt()
    {
        vfx.transform.localPosition = tower.PreviousPieceLocalPosition;
        vfx.Play();
    }

    private void OnDisable()
    {
        tower.PieceBuilt -= OnPieceBuilt;
    }
}