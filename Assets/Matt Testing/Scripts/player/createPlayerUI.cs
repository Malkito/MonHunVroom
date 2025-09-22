using UnityEngine;
using Unity.Netcode;
public class createPlayerUI : NetworkBehaviour
{
    [SerializeField] private GameObject HUD;
    private void Start()
    {
        if (IsLocalPlayer)
        {
            HUD.SetActive(true);
            print("Hud activated");
        }
    }
}
