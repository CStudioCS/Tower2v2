using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class LocalProxyCheckInOut : MonoBehaviour
{
    private PlayerInput localInput;

    private void Start()
    {
        localInput = GetComponent<PlayerInput>();

        if (LobbyManager.Instance != null && localInput != null)
            LobbyManager.Instance.OnPlayerJoined(localInput);
    }

    private void OnDestroy()
    {
        if (LobbyManager.Instance != null && localInput != null)
            LobbyManager.Instance.OnPlayerLeft(localInput);
    }
}