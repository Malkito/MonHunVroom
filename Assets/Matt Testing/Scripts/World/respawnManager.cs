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

        setRefrencesAndActions(player, false);


        rb.linearVelocity = new Vector3(0,0,0);



        yield return new WaitForSeconds(respawnTime);

        if(numberOfPlayersDead != NetworkManager.Singleton.ConnectedClients.Count)
        {
            respawnPlayer(player);
        }

    } 

    public void respawnPlayer(Transform player)
    {
        StopCoroutine(StartSpawnPlayer(player));
        setRefrencesAndActions(player, true);
        player.transform.position = respawnPoints[Random.Range(0, respawnPoints.Length)].transform.position;
        ///Move Camera

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



    private void setRefrencesAndActions(Transform player, bool setBool)
    {
        tankMovement movement = player.GetComponent<tankMovement>();
        tankCameraMovement TankCam = player.GetComponent<tankCameraMovement>();
        playerUpgradeManager upgrade = player.GetComponent<playerUpgradeManager>();
        playerShooting shooting = player.GetComponent<playerShooting>();
        playerHealth health = player.GetComponent<playerHealth>();
        player.transform.position = respawnPoints[Random.Range(0, respawnPoints.Length)].transform.position;
        ///Move Camera
        movement.canMove = setBool;
        upgrade.canUseUpgrade = setBool;
        shooting.canShoot = setBool;
        health.canTakeDamage = setBool;
        TankCam.canMove = setBool;

        health.currentHealth = health.maxHealth;
    }


}
