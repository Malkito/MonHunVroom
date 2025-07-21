using UnityEngine;

[System.Serializable]
public class Timer
{
    public delegate void TimerFinishedHandler();

    [SerializeField]
    [Min(0)]
    private float _durationSeconds;

    private float _elapsedTime;

    public TimerFinishedHandler OnTimerFinished { get; set; }

    public bool IsComplete { get { return _elapsedTime >= _durationSeconds; } }

    public Timer(float durationSeconds)
    {
        _durationSeconds = durationSeconds;
        Reset();
    }

    public void Reset()
    {
        _elapsedTime = 0f;
    }

    public void Update(float timeSinceLastStep)
    {
        if (IsComplete)
        {
            OnTimerFinished?.Invoke();
            Reset();
        }
        else
        {
            _elapsedTime += timeSinceLastStep;
        }
    }

    public void Update()
    {
        Update(Time.deltaTime);
    }
}
