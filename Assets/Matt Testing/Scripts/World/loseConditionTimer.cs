using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class loseConditionTimer : NetworkBehaviour
{
    [SerializeField] private float maxTimerInSeconds;

    private float currentTime;

    [SerializeField] private TMP_Text timerText; 


    private void Start()
    {
        currentTime = maxTimerInSeconds;
    }
    void Update()
    {
        if(GameStateManager.Instance.CurrentState == GameStateManager.State.GamePlaying && currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);

            if(currentTime <= 0)
            {
                gameOver();

            }
        }
    }


    private void gameOver()
    {
        GameStateManager.Instance.setNewState(GameStateManager.State.GameOver);
    }






}
