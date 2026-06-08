using System;
using UnityEngine;

[System.Serializable]
public class Timer
{
    [SerializeField]
    [Min(0)]
    private float _durationSeconds;

    private float _elapsedTime;

    private Action _onFinished;

    private bool _resetOnFinished;

    public bool HasFinished { get => _elapsedTime >= _durationSeconds; }

    public float TimeElapsed {  get { return _elapsedTime; } }

    public float Duration { get { return _durationSeconds; } }

    public Timer(float duration, Action onFinished, bool resetOnFinished = true)
    {
        _durationSeconds = Mathf.Max(duration, 0);
        _elapsedTime = 0f;

        _onFinished = onFinished;

        _resetOnFinished = resetOnFinished;
    }
    public Timer(float duration, bool resetOnFinished = true) : this(duration, null, resetOnFinished)
    {

    }

    public Timer(Action onFinished, bool resetOnFinished = true) : this(0, onFinished, resetOnFinished)
    {

    }

    public void SetDuration(float duration)
    {
        _durationSeconds = Mathf.Max(duration, 0);
        _elapsedTime = 0f;
    }

    public void Reset()
    {
        _elapsedTime = 0f;
    }

    public void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (HasFinished)
        {
            _onFinished?.Invoke();
            
            if (_resetOnFinished)
            {
                _elapsedTime = 0f;
            }
        }
    }
}
