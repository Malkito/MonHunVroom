using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
public class MainMenuManager : NetworkBehaviour
{


    private bool isDay;
    [SerializeField] Material[] skyBoxMats;
    [SerializeField] private Light directionalLights;
    [SerializeField] private GameObject[] streetLights;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button goToLobby;
    [SerializeField] private GameObject lobbyCreation;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject levelSelect;

    public void StartGame()
    {
        if (isDay)
        {
            Loader.LoadNetwork(Loader.Scene.DayGameScene);
        }
        else
        {
            Loader.LoadNetwork(Loader.Scene.NightGameScene);
        }
    }

    public void goToLevelSelect()
    {
        goToLobby.gameObject.SetActive(false);
        lobbyCreation.SetActive(true);
        quitButton.gameObject.SetActive(false);
    }


    public void setDay()
    {
        isDay = true;
        RenderSettings.skybox = skyBoxMats[0];

        RenderSettings.ambientLight = Color.grey;


        foreach (GameObject streetLight in streetLights)
        {
            streetLight.SetActive(false);
        }

        startGameButton.interactable = true;


    }

    public void setNight()
    {
        isDay = false;
        RenderSettings.skybox = skyBoxMats[1];

        RenderSettings.ambientLight = Color.black;

        foreach (GameObject streetLight in streetLights)
        {
            streetLight.SetActive(true);
        }
        startGameButton.interactable = true;
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
