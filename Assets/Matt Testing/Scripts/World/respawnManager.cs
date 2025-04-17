using UnityEngine;
using System.Collections;

public class respawnManager : MonoBehaviour
{
    
    [SerializeField] private GameObject[] respawnPoints;
    private static respawnManager instance;
    [SerializeField] private float respawnTime;


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
    // Update is called once per frame
    void Update()
    {
        
    }


    public IEnumerator respawnPlayer(Transform player)
    {
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


        ///Despawn Player
        ///move Camera to cinematic


        yield return new WaitForSeconds(respawnTime);


        player.transform.position = respawnPoints[Random.Range(0, respawnPoints.Length)].transform.position;

        ///Move Camera

        movement.canMove = true;
        upgrade.canUseUpgrade = true;
        shooting.canShoot = true;
        health.canTakeDamage = true;

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

}
