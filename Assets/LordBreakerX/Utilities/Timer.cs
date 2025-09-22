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

    public Timer() 
    {
    }

    public Timer(float durationSeconds)
    {
        SetDuration(durationSeconds);
    }

    public void SetDuration(float duration)
    {
        _durationSeconds = Mathf.Max(duration, 0);
        Reset();
    }

    public void Reset()
    {
        _elapsedTime = 0f;
    }

    public void Update(bool resetOnComplete = true)
    {
        _elapsedTime += Time.deltaTime;

        if (IsComplete)
        {
            OnTimerFinished?.Invoke();
            if (resetOnComplete) Reset();
        }
    }
}
