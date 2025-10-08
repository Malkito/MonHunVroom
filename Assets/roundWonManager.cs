using UnityEngine;
using Unity.Netcode;

public class roundWonManager : NetworkBehaviour
{
    [SerializeField] private GameObject nextRoundPortal;

    private int playersReadyForNextRound;
    private NetworkObject playerNetOBJ;
    private GameObject playerObj;

    private void Start()
    {
        playersReadyForNextRound = 0;
    }

    void Update()
    {
        if(GameStateManager.Instance.CurrentState == GameStateManager.State.RoundWon)
        {
            TurnOnRoundWOnPortalClientRpc();
        }
    }

    [ClientRpc]
    private void TurnOnRoundWOnPortalClientRpc()
    {
        nextRoundPortal.SetActive(true);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNetOBJ = other.GetComponent<NetworkObject>();
            playerObj = other.gameObject;
            playersReadyForNextRound++;
            despawnCharacterServerRpc();
            if (playersReadyForNextRound == NetworkManager.Singleton.ConnectedClients.Count)
            {
                Loader.LoadNetwork(Loader.Scene.DayGameScene);
                enableMouseClientRpc();

            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void despawnCharacterServerRpc()
    {
        playerNetOBJ.Despawn();
        Destroy(playerObj);
        playerNetOBJ = null;
    }

    [ClientRpc]
    private void enableMouseClientRpc()
    {
        Cursor.visible=true;
        Cursor.lockState = CursorLockMode.Confined;
    }

}
