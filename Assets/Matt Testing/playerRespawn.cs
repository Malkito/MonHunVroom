using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class playerRespawn : MonoBehaviour
{
    /// <summary>
    /// 
    /// Handles the respawning for a player
    /// 
    /// When the health reaches 0, all input is ignored, then after a delay the player is moved to a random spawn location, input is resumed
    /// 
    /// </summary>
    private tankMovement movement;
    private tankCameraMovement TankCam;
    private playerUpgradeManager upgrade;
    private playerShooting shooting;
    private playerHealth health;

    private GameObject[] respawnPoints;

    [SerializeField] private TMP_Text countdownText;

    [SerializeField] private GameObject deathUI;

    [SerializeField] private float respawnTime;

    public bool isDead; // public flag used for lose condition

    private int countdownTime;

    //private Coroutine countdownCoroutine;

    [SerializeField] private Rigidbody rb;

    private void Awake()
    {

        //sets player action refrecenes
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
        setPlayerActions(false); // Stops the player actions

        StartCoroutine(CountdownRoutine(countdownTime)); // activates the UI

        rb.linearVelocity = new Vector3(0, 0, 0); // stops the player velocity

        yield return new WaitForSeconds(respawnTime); // wait for respawn delay

        endRespawn();
    }





    private void endRespawn()
    {
        setPlayerActions(true); //resuems player actions
        transform.position = respawnPoints[Random.Range(0, respawnPoints.Length)].transform.position; // Moves the player to a respawn point
        deathUI.SetActive(false); // de-activates the UI
        isDead = false; 
    }



    private void setPlayerActions(bool setBool) //stops / restarts the player from being able to shoot, use upgrades, shoot, takening damage and movign 
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

    private IEnumerator CountdownRoutine(int seconds) // Handles the Respawn countdown UI
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
