using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
public class MainMenuManager : NetworkBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button goToLobby;
    [SerializeField] private GameObject lobbyCreation;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject levelSelect;

    private void Awake()
    {
        goToLobby.onClick.AddListener(() =>
        {
            goToLevelSelect();
        });
    }

    public void StartGame()
    {
        Loader.LoadNetwork(Loader.Scene.DayGameScene);
    }



    public void goToLevelSelect()
    {
        print("Button being pressed");
        goToLobby.gameObject.SetActive(false);
        lobbyCreation.SetActive(true);
        quitButton.gameObject.SetActive(false);
    }

    public void exitGame() 
    {
        Application.Quit();
    }

    public void mainMenu()
    {
        SceneManager.LoadScene(0);
    }


    public void backButton()
    {
        quitButton.gameObject.SetActive(true);
        lobbyCreation.SetActive(false);
        goToLobby.gameObject.SetActive(true);

    }



    public void spawnlevelSelect()
    {
        levelSelect.SetActive(true);
    }







}
