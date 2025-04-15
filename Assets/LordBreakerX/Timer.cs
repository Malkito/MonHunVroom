using UnityEngine;

[System.Serializable]
public class Timer
{
    public delegate void TimerEnded();

    [SerializeField]
    [Min(0)]
    private float _timeInSeconds;

    private float _time;

    public TimerEnded onTimerFinished;

    public bool IsFinished { get { return _time >= _timeInSeconds; } }

    public Timer(float timeInSeconds)
    {
        _timeInSeconds = timeInSeconds;
        _time = 0;
    }

    public void Reset()
    {
        _time = 0;
    }

    public void Step(float timeSinceLastStep)
    {
        if (IsFinished)
        {
            onTimerFinished?.Invoke();
            Reset();
        }
        else
        {
            _time += timeSinceLastStep;
        }
    }

    public void Step()
    {
        Step(Time.deltaTime);
    }
}
