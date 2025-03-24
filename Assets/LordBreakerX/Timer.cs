using UnityEngine;

[System.Serializable]
public class Timer
{
    public delegate void TimerEnded();

    [SerializeField]
    [Min(0)]
    private float _timeInSeconds;

    private float _time;

    public TimerEnded OnTimerFinished { get; set; } 

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
        if (_time >= _timeInSeconds)
        {
            OnTimerFinished?.Invoke();
            Reset();
        }
        else
        {
            _time += timeSinceLastStep;
        }
    }
}
