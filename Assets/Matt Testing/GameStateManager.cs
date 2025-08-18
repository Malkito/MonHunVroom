using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
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

    private State CurrentState;

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
        turnOffLoseUIClientRpc();
        CurrentState = State.GamePlaying;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if(waitingToStartTimer <0f)
                {
                    CurrentState = State.CountdownToStart;
                }
                break;
            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    CurrentState = State.GamePlaying;
                }
                break;
            case State.GamePlaying:
                break;
            case State.GameOver:
                spawnLoseUIClientRpc();
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
 