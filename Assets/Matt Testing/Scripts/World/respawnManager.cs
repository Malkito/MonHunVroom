using UnityEngine;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class respawnManager : NetworkBehaviour
{
    
    public GameObject[] respawnPoints;
    private static respawnManager instance;
    [SerializeField] private float respawnTime;
    private int countdownTime;
    private int numberOfPlayersDead;

    [SerializeField] private TMP_Text countdownText;
    private Coroutine countdownCoroutine;

    [SerializeField] private GameObject deathUI;

    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        countdownTime = Mathf.RoundToInt(respawnTime);
        getSpawnPoints();
    }


    public static respawnManager Instance
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


    private void Update()
    {
        if (NetworkManager.Singleton == null) return;
        if (numberOfPlayersDead == NetworkManager.Singleton.ConnectedClients.Count && NetworkManager.Singleton.IsServer)
        {
            gameLost();
        }
    }

    private void StartCountdown(int seconds)
    {
        deathUI.SetActive(true);

        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(CountdownRoutine(seconds));
    }
    private IEnumerator CountdownRoutine(int seconds)
    {
        int timeLeft = seconds;
        while (timeLeft > 0)
        {
            countdownText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        countdownText.text = "0";
    }




    public IEnumerator StartSpawnPlayer(Transform player)
    {
        numberOfPlayersDead++;
        Rigidbody rb = player.gameObject.GetComponent<Rigidbody>();
        setRefrencesAndActions(player, false);
        rb.linearVelocity = new Vector3(0,0,0);

        deathUI = FindChildByTag(player, "RespawnUI");
        Transform textOBJ = deathUI.transform.Find("Number");
        countdownText = textOBJ.gameObject.GetComponent<TMP_Text>();                                              
        StartCountdown(countdownTime);


        yield return new WaitForSeconds(respawnTime);
        if (numberOfPlayersDead != NetworkManager.Singleton.ConnectedClients.Count)
        {
            respawnPlayer(player);
            deathUI.SetActive(false);
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

    private GameObject FindChildByTag(Transform parent, string tag)  
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
                return child.gameObject;

            // Recursively search nested children
            GameObject found = FindChildByTag(child, tag);
            if (found != null)
                return found;
        }

        return null;
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

        movement.canMove = setBool;
        upgrade.canUseUpgrade = setBool;
        shooting.canShoot = setBool;
        health.canTakeDamage = setBool;
        TankCam.canMove = setBool;

        health.currentHealth = health.maxHealth;
    }


}
