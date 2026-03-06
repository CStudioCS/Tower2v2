using System;
using UnityEngine;
using UnityEngine.VFX;

public class FurnaceView : MonoBehaviour
{
    [SerializeField] private Furnace furnace;
    
    private static readonly int Cooking = Animator.StringToHash("Cooking");
    [SerializeField] private Animator animator;

    [SerializeField] private VisualEffect[] vfxs;

    private void OnEnable()
    {
        furnace.StartedCooking += OnStartedCooking;
        furnace.StoppedCooking += OnStoppedCooking;
        StopVfxs();
    }

    private void OnStartedCooking()
    {
        Animate();
        PlayVfxs();
    }

    private void OnStoppedCooking()
    {
        Animate(false);
        StopVfxs();
    }
    
    private void Animate(bool animate = true) => animator.SetBool(Cooking, animate);

    private void PlayVfxs()
    {
        foreach (VisualEffect vfx in vfxs)
            vfx.Play();
    }
    
    private void StopVfxs()
    {
        foreach (VisualEffect vfx in vfxs)
            vfx.Stop();
    }

    private void OnDisable()
    {
        furnace.StartedCooking -= OnStartedCooking;
        furnace.StoppedCooking -= OnStoppedCooking;
    }
}