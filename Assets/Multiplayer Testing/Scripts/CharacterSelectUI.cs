using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button readyButton;


    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.load(Loader.Scene.MainMenu);
        });

        readyButton.onClick.AddListener(() =>
        {
            CharacterSelectReady.Instance.SetPLayerReady();
        });


    }



}
