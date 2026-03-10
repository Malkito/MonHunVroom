using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class roundWonManager : NetworkBehaviour
{
    [SerializeField] private GameObject nextRoundPortal;

    private int playersReadyForNextRound;
    private NetworkObject playerNetOBJ;
    private GameObject playerObj;
    private SphereCollider coli;

    [SerializeField] private Loader.Scene tronScene;
    [SerializeField] private Loader.Scene fantasyScene;

    private Loader.Scene currentScene;


    private void Awake()
    {
        coli = gameObject.GetComponent<SphereCollider>();
    }
    private void Start()
    {
        playersReadyForNextRound = 0;
        coli.enabled = false;

        if(SceneManager.GetActiveScene().name == tronScene.ToString())
        {
            currentScene = tronScene;
        }
        else if(SceneManager.GetActiveScene().name == fantasyScene.ToString())
        {
            currentScene = fantasyScene;
        }


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
        coli.enabled = true;
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

                if(currentScene == tronScene)
                {
                    goToNextScene(fantasyScene);
                }
                else if(currentScene == fantasyScene)
                {
                    goToNextScene(tronScene);
                }
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



    private void goToNextScene(Loader.Scene nextScene)
    {
        Loader.LoadNetwork(nextScene);

    }

}
