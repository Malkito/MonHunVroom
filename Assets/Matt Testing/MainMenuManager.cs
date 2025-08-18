using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenuManager : MonoBehaviour
{


    private bool isDay;
    [SerializeField] Material[] skyBoxMats;
    [SerializeField] private Light directionalLights;
    [SerializeField] private GameObject[] streetLights;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button levelSelectButton;
    [SerializeField] private GameObject levelSelect;
 
    public void StartGame()
    {
        if (isDay)
        {
            SceneManager.LoadScene(1);
        }
        else
        {
            SceneManager.LoadScene(2);
        }
    }

    public void goToLevelSelect()
    {
        levelSelectButton.interactable = false;
        levelSelect.SetActive(true);

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


    







}
