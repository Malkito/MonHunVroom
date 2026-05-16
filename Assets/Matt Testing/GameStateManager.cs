using UnityEngine;
using Unity.Netcode;
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
    private State previousState;

    [SerializeField] private float waitingToStartTimer;
    [SerializeField] private float countdownToStartTimer;

    public static int LevelsCompleted { get; private set; } = -1;
    private static GameStateManager instance;

    [SerializeField] private GameObject loseUI;
    [SerializeField] private TMP_Text gameStateText;
    [SerializeField] private monsterDeathLootDrop monDeathLoot;

    public static GameStateManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("GameStateManager is null");
            }
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;

        LevelsCompleted += 1;
        Debug.Log("Levels Completed: " +  LevelsCompleted);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            setNewState(State.WaitingToStart);
        }
    }

    private void Update()
    {
        if (!IsSpawned) return;

        // Run logic only when state changes
        if (CurrentState != previousState)
        {
            HandleStateEntered(CurrentState);
            previousState = CurrentState;
        }

        // Continuous logic
        if (CurrentState == State.GamePlaying)
        {
            GameInput.instance.enableOrDisablePlayerAction(true);
        }

        gameStateText.text = CurrentState.ToString();
    }

    private void HandleStateEntered(State newState)
    {
        switch (newState)
        {
            case State.WaitingToStart:
                Debug.Log("Waiting for players...");
                break;

            case State.CountdownToStart:
                Debug.Log("Countdown started");
                break;

            case State.GamePlaying:
                Debug.Log("Game started");
                break;

            case State.GameOver:
                if (IsServer)
                {
                    ResetScene();
                }
                break;

            case State.RoundWon:
                if (IsServer)
                {
                    monDeathLoot.spawnobjects();
                }
                break;
        }
    }

    public void setNewState(State newState)
    {
        if (!IsServer) return;

        CurrentState = newState;
        SetNewStateClientRpc((int)newState);
    }

    [ClientRpc]
    private void SetNewStateClientRpc(int newStateNum)
    {
        CurrentState = (State)newStateNum;
    }

    private void ResetScene()
    {
        if (SceneManager.GetActiveScene().name == Loader.Scene.TronGameScene.ToString())
        {
            Loader.LoadNetwork(Loader.Scene.TronGameScene);
        }
        else if (SceneManager.GetActiveScene().name == Loader.Scene.FantasyGameScene.ToString())
        {
            Loader.LoadNetwork(Loader.Scene.FantasyGameScene);
        }
    }

    [ClientRpc]
    private void SpawnLoseUIClientRpc()
    {
        loseUI.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    [ClientRpc]
    private void TurnOffLoseUIClientRpc()
    {
        loseUI.SetActive(false);
    }
}