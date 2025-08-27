using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TestingLobbyUI : NetworkBehaviour
{

    [SerializeField] private Button createGameButton;
    [SerializeField] private Button joinGameButton;


    private void Awake()
    {
       createGameButton.onClick.AddListener(() => {
           NetworkManager.StartHost();
           Loader.LoadNetwork(Loader.Scene.CharacterSelect);
       });


        joinGameButton.onClick.AddListener(() => {
            NetworkManager.StartClient();
        });
    }


}
