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
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private GameObject levelSelect;
    [SerializeField] private GameObject quitButton;
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
        levelSelectButton.gameObject.SetActive(false);
        levelSelect.SetActive(true);
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
        levelSelect.SetActive(false);
        levelSelectButton.gameObject.SetActive(true);

    }







}
