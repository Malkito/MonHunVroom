using UnityEngine;
using Unity.Netcode;
public class createPlayerUI : NetworkBehaviour
{
    [SerializeField] private GameObject HUD;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner || !IsLocalPlayer) return;
        HUD.SetActive(true);
    }
}
