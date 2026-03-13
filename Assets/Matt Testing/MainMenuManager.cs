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
    [SerializeField] private GameObject[] Cameras;
    int levelNum;

    [SerializeField] private GameObject[] Titles;

    bool isDay;

    public void StartGame()
    {

        Loader.LoadNetwork(Loader.Scene.TronGameScene);
        /*
        switch (levelNum)
        {
            case 0:
                Loader.LoadNetwork(Loader.Scene.DayGameScene);
                break;
            case 1:
                Loader.LoadNetwork(Loader.Scene.TronGameScene);
                break;
            case 2:
                Loader.LoadNetwork(Loader.Scene.FantasyGameScene);
                break;
        }
        */
    }



    public void goToLobby()
    {
        print("Button being pressed");
        goToLobbyButton.gameObject.SetActive(false);
        lobbyCreation.SetActive(true);
        quitButton.gameObject.SetActive(false);
        foreach(GameObject obj in Titles)
        {
            obj.SetActive(false);
        }

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
        foreach (GameObject obj in Titles)
        {
            obj.SetActive(true);
        }

    }
    public void spawnlevelSelect()
    {
        levelSelect.SetActive(true);
    }

    public void setDay()
    {
        changeCameraSetStartButton(0);
    }
    public void setTron()
    {
        changeCameraSetStartButton(1);
    }

    public void setFantasy()
    {
        changeCameraSetStartButton(2);
    }
    private void changeCameraSetStartButton(int levelNumber)
    {
        levelNum = levelNumber;
        
        for (int i = 0; i < Cameras.Length; i++)
        {
            if (levelNumber == i)
            {
                Cameras[i].SetActive(true);
            }
            else
            {
                Cameras[i].SetActive(false);
            }
        }
    }
}
