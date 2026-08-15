using UnityEngine;
using Unity.Netcode;
using TMPro;

public class loseConditionTimer : NetworkBehaviour
{
    [SerializeField] private float maxTimerInSeconds;
    [SerializeField] private TMP_Text timerText;

    private NetworkVariable<float> currentTime = new NetworkVariable<float>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentTime.Value = maxTimerInSeconds;
        }

        UpdateDisplay(currentTime.Value);

        currentTime.OnValueChanged += OnTimeChanged;
    }

    private void OnDestroy()
    {
        currentTime.OnValueChanged -= OnTimeChanged;
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if (GameStateManager.Instance.CurrentState != GameStateManager.State.GamePlaying)
            return;

        if (currentTime.Value <= 0)
            return;

        currentTime.Value -= Time.deltaTime;

        if (currentTime.Value <= 0)
        {
            currentTime.Value = 0;
            gameOver();
        }
    }

    private void OnTimeChanged(float previous, float current)
    {
        UpdateDisplay(current);
    }

    private void UpdateDisplay(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        timerText.text = $"{minutes:00} : {seconds:00}";
    }

    private void gameOver()
    {
        GameStateManager.Instance.setNewState(GameStateManager.State.GameOver);
    }
}