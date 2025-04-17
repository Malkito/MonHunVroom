using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;


public class Loader : MonoBehaviour
{


    public enum Scene
    {
        MainMenu,
        PVPScene,
        LobbyScene,
        CharacterSelcet,
        MainScene,

    }


    private static Scene targetScene;

    public static void load(Scene targetScene)
    {
        Loader.targetScene = targetScene;
        SceneManager.LoadScene(targetScene.ToString());
    }

    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }


}
