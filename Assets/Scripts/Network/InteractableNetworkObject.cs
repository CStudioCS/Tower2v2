using Fusion;
using UnityEngine;

[RequireComponent(typeof(Interactable), typeof(NetworkObject))]
public class InteractableNetworkObject : NetworkBehaviour
{
    private Interactable interactable;

    private void Awake()
    {
        interactable = GetComponent<Interactable>();
    }

    public override void Spawned()
    {
        // Object.Id.Raw est un entier unique garanti par Fusion
        interactable.RegisterNetworkId(((int)Object.Id.Raw));
    }
}