using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class playerRespawn : MonoBehaviour
{

    private tankMovement movement;
    private tankCameraMovement TankCam;
    private playerUpgradeManager upgrade;
    private playerShooting shooting;
    private playerHealth health;

    private GameObject[] respawnPoints;

    [SerializeField] private TMP_Text countdownText;

    [SerializeField] private GameObject deathUI;

    [SerializeField] private float respawnTime;

    public bool isDead;

    private int countdownTime;

    //private Coroutine countdownCoroutine;

    [SerializeField] private Rigidbody rb;

    private void Awake()
    {
        movement = gameObject.GetComponent<tankMovement>();
        TankCam = gameObject.GetComponent<tankCameraMovement>();
        upgrade = gameObject.GetComponent<playerUpgradeManager>();
        shooting = gameObject.GetComponent<playerShooting>();
        health = gameObject.GetComponent<playerHealth>();

        respawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

    }
    void Start()
    {
        countdownTime = Mathf.RoundToInt(respawnTime);
    }


    void Update()
    {
        if(health.currentHealth <= 0)
        {
            StartCoroutine(startRespawn());
        }
    }

    private IEnumerator startRespawn()
    {
        Debug.Log("Start respawn");
        isDead = true;
        setPlayerActions(false);
        StartCoroutine(CountdownRoutine(countdownTime));
        rb.linearVelocity = new Vector3(0, 0, 0);

        yield return new WaitForSeconds(respawnTime);

        endRespawn();



    }





    private void endRespawn()
    {
        setPlayerActions(true);
        transform.position = respawnPoints[Random.Range(0, respawnPoints.Length)].transform.position;
        deathUI.SetActive(false);
        isDead = false;
    }



    private void setPlayerActions(bool setBool)
    {
        movement.canMove = setBool;
        upgrade.canUseUpgrade = setBool;
        shooting.canShoot = setBool;
        health.canTakeDamage = setBool;
        TankCam.canMove = setBool;

        health.currentHealth = health.maxHealth;
    }


    /*
    private void startCountdown(int seconds)
    {
        deathUI.SetActive(true);

        if (countdownCoroutine != null) 
        { 
            StopCoroutine(countdownCoroutine);
        }

        countdownCoroutine = StartCoroutine(CountdownRoutine(seconds));
    }
    */

    private IEnumerator CountdownRoutine(int seconds)
    {
        print("Countdonw routiune working");
        deathUI.SetActive(true);
        int timeLeft = seconds;
        while (timeLeft > 0)
        {
            countdownText.text = timeLeft.ToString();
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
        countdownText.text = "0";
    }







}
