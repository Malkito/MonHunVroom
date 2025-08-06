using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class respawnManager : NetworkBehaviour
{
    
    [SerializeField] private GameObject[] respawnPoints;
    private static respawnManager instance;
    [SerializeField] private float respawnTime;
    private int numberOfPlayersDead;
    private bool serverStarted;


    public static respawnManager Instance
    {
        get
        {
            if(instance == null)
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


    void Start()
    {
        getSpawnPoints();
    }

 

    private void Update()
    {
        if (numberOfPlayersDead == NetworkManager.Singleton.ConnectedClients.Count && NetworkManager.Singleton.IsServer)
        {
            gameLost();
        }
    }


    public IEnumerator StartSpawnPlayer(Transform player)
    {
        numberOfPlayersDead++;

        Rigidbody rb = player.gameObject.GetComponent<Rigidbody>();
        playerMovement movement = player.GetComponent<playerMovement>();
        playerUpgradeManager upgrade = player.GetComponent<playerUpgradeManager>();
        playerShooting shooting = player.GetComponent<playerShooting>();
        playerHealth health = player.GetComponent<playerHealth>();

        health.currentHealth = health.maxHealth;

        rb.linearVelocity = new Vector3(0,0,0);

        movement.canMove = false;
        upgrade.canUseUpgrade = false;
        shooting.canShoot = false;
        health.canTakeDamage = false;


        yield return new WaitForSeconds(respawnTime);

        if(numberOfPlayersDead != NetworkManager.Singleton.ConnectedClients.Count)
        {
            respawnPlayer(player);
        }

    } 

    public void respawnPlayer(Transform player)
    {
        StopCoroutine(StartSpawnPlayer(player));
        playerMovement movement = player.GetComponent<playerMovement>();
        playerUpgradeManager upgrade = player.GetComponent<playerUpgradeManager>();
        playerShooting shooting = player.GetComponent<playerShooting>();
        playerHealth health = player.GetComponent<playerHealth>();
        player.transform.position = respawnPoints[Random.Range(0, respawnPoints.Length)].transform.position;
        ///Move Camera
        movement.canMove = true;
        upgrade.canUseUpgrade = true;
        shooting.canShoot = true;
        health.canTakeDamage = true;

        numberOfPlayersDead--;

    }

    private void getSpawnPoints()
    {
        int childCount = transform.childCount;
        respawnPoints = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            respawnPoints[i] = transform.GetChild(i).gameObject;

        }
    }

    private void gameLost()
    {
        GameStateManager.Instance.setNewState(GameStateManager.State.GameOver);
    }


}
