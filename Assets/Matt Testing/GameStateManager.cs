using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GameStateManager : NetworkBehaviour
{
    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
        RoundWon
    }

    public State CurrentState;

    [SerializeField] private float waitingToStartTimer;
    [SerializeField] private float countdownToStartTimer;
    private static GameStateManager instance;
    [SerializeField] GameObject loseUI;
    [SerializeField] TMP_Text gameStateText;
    [SerializeField] private monsterDeathLootDrop monDeathLoot;


    public static GameStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError(message: "RespawnManager is null");
            }
            return instance;
        }
    }


    private void Awake()
    {
        instance = this;

    }

    private void Start()
    {  
       // turnOffLoseUIClientRpc();
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                break;
            case State.GamePlaying:
                GameInput.instance.enableOrDisablePlayerAction(true);
                break;
            case State.GameOver:
                resetSceneServerRpc();
                //spawnLoseUIClientRpc();
                break;
            case State.RoundWon:
                monDeathLoot.spawnobjects();
                break;

        }
        gameStateText.text = CurrentState.ToString();

    }

    public void setNewState(State newState)
    {
        CurrentState = newState;
    }


    [ServerRpc]
    private void resetSceneServerRpc()
    {
        //PlayerInputActions.Player.Disable();
        if(SceneManager.GetActiveScene().name == Loader.Scene.TronGameScene.ToString())
        {
            Loader.LoadNetwork(Loader.Scene.TronGameScene);

        } else if (SceneManager.GetActiveScene().name == Loader.Scene.FantasyGameScene.ToString())
        {
            Loader.LoadNetwork(Loader.Scene.FantasyGameScene);
        }


    }


    [ClientRpc]
    private void spawnLoseUIClientRpc()
    {
        loseUI.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    [ClientRpc]
    private void turnOffLoseUIClientRpc()
    {
        loseUI.SetActive(false);
    }


}
 