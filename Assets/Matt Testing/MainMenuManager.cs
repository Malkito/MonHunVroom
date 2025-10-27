using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
public class MainMenuManager : NetworkBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button goToLobbyButton;
    [SerializeField] private GameObject lobbyCreation;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject levelSelect;


    public void StartGame()
    {
        Loader.LoadNetwork(Loader.Scene.DayGameScene);
    }



    public void goToLobby()
    {
        print("Button being pressed");
        goToLobbyButton.gameObject.SetActive(false);
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
        goToLobbyButton.gameObject.SetActive(true);

    }
    public void spawnlevelSelect()
    {
        levelSelect.SetActive(true);
    }

    public void setDay()
    {
        startGameButton.interactable = true;
    } 







}
