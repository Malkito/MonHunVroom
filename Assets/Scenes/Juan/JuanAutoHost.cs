using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Test helper for Juan's scene copies only. The team's game scenes ship with the
/// MultiplayerManager (NetworkManager) disabled because the session is started from the MainMenu.
/// This enables it in Awake and starts a Host in Start (before other scripts' Start, e.g. readyCheck)
/// when the scene is played directly in the Editor.
/// </summary>
[DefaultExecutionOrder(-10000)]
public class JuanAutoHost : MonoBehaviour
{
    private NetworkManager _manager;

    private void Awake()
    {
        if (!Application.isEditor)
        {
            return;
        }

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            return; // Session already started (e.g. coming from the MainMenu).
        }

        NetworkManager[] managers = FindObjectsByType<NetworkManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (managers.Length == 0)
        {
            Debug.LogWarning("[JuanAutoHost] No NetworkManager found in the scene.");
            return;
        }

        _manager = managers[0];
        _manager.gameObject.SetActive(true);
    }

    private void Start()
    {
        if (_manager != null && !_manager.IsListening && !_manager.StartHost())
        {
            Debug.LogError("[JuanAutoHost] StartHost failed.");
        }
    }
}
