using System.Collections.Generic;
using LordBreakerX.AttackSystem;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAttackAnimator : MonoBehaviour
{
    private const string LOCOMOTION = "Locomotion";
    private const string LOCOMOTION_SPEED = "LocomotionSpeed";
    private const string ATTACK_SPEED = "AttackSpeed";

    private static readonly int LocomotionHash = Animator.StringToHash(LOCOMOTION);
    private static readonly int LocomotionSpeedHash = Animator.StringToHash(LOCOMOTION_SPEED);
    private static readonly int AttackSpeedHash = Animator.StringToHash(ATTACK_SPEED);

    [SerializeField] private AttackController _attackController;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private NetworkAnimator _networkAnimator;
    [SerializeField] private float _arriveDistance = 1f;
    [SerializeField] private AttackAnimationEntry[] _entries;
    [SerializeField, Range(0.25f, 2f)] private float _walkAnimSpeed = 1f;
    [SerializeField, Range(0.25f, 2f)] private float _runAnimSpeed = 1f;
    [SerializeField, Range(0f, 1f)] private float _locomotionDamp = 0.15f;
    [SerializeField, Min(0f)] private float _movingThreshold = 0.1f;

    private Dictionary<System.Type, AttackAnimationEntry> _entriesByType;
    private AttackAnimationEntry _currentEntry;
    private bool _fired;
    private int _framesSinceStart;
    private float _activeClock;
    private int _nextRepeatIndex = 1;
    private bool _hasLocomotionParameter;
    private bool _hasLocomotionSpeedParameter;
    private bool _hasAttackSpeedParameter;

    private void Awake()
    {
        CacheFloatParameters();
        _entriesByType = new Dictionary<System.Type, AttackAnimationEntry>();

        if (_entries == null)
            return;

        foreach (AttackAnimationEntry entry in _entries)
        {
            if (entry == null || entry.attack == null)
                continue;

            System.Type attackType = entry.attack.GetType();
            if (!_entriesByType.ContainsKey(attackType))
                _entriesByType.Add(attackType, entry);
        }
    }

    private void OnEnable()
    {
        _attackController.AttackStarted += HandleStarted;
        _attackController.AttackStopped += HandleStopped;
    }

    private void OnDisable()
    {
        _attackController.AttackStarted -= HandleStarted;
        _attackController.AttackStopped -= HandleStopped;
    }

    private void HandleStarted(ScriptableAttack attack)
    {
        _currentEntry = FindEntry(attack);
        _fired = false;
        _framesSinceStart = 0;
        _activeClock = 0f;
        _nextRepeatIndex = 1;

        if (_currentEntry != null && _currentEntry.moment == TriggerMoment.OnStart)
            Fire(_currentEntry);
    }

    private void HandleStopped(ScriptableAttack attack)
    {
        if (_currentEntry != null && _currentEntry.moment == TriggerMoment.OnEnd && !_fired)
            Fire(_currentEntry);

        _currentEntry = null;
        _activeClock = 0f;
        _nextRepeatIndex = 1;
    }

    private void Update()
    {
        if (!_attackController.IsServer)
            return;

        UpdateLocomotion();
        UpdateAttackAnimation();
    }

    private void UpdateLocomotion()
    {
        if (_animator == null || _agent == null)
            return;

        bool moving = _agent.velocity.magnitude > _movingThreshold;
        float target = moving ? (_attackController.IsAttacking ? 2f : 1f) : 0f;
        if (_hasLocomotionParameter)
            _animator.SetFloat(LocomotionHash, target, _locomotionDamp, Time.deltaTime);

        float current = _hasLocomotionParameter ? _animator.GetFloat(LocomotionHash) : 0f;
        float speed = current > 1f
            ? Mathf.Lerp(_walkAnimSpeed, _runAnimSpeed, Mathf.Clamp01(current - 1f))
            : _walkAnimSpeed;

        if (_hasLocomotionSpeedParameter)
            _animator.SetFloat(LocomotionSpeedHash, speed);
    }

    private void UpdateAttackAnimation()
    {
        if (_currentEntry == null || _currentEntry.moment != TriggerMoment.OnArrive)
            return;

        bool arrived = !_agent.pathPending && _agent.remainingDistance <= _arriveDistance;
        if (_framesSinceStart >= 1 && arrived)
        {
            if (_currentEntry.repeatWhileActive)
            {
                _activeClock += Time.deltaTime;
                TryFireRepeatingEntry(_currentEntry);
            }
            else if (!_fired)
            {
                Fire(_currentEntry);
            }
        }

        _framesSinceStart++;
    }

    private void TryFireRepeatingEntry(AttackAnimationEntry entry)
    {
        float speed = entry.speed > 0f ? entry.speed : 1f;
        float repeatInterval = entry.repeatInterval > 0f ? entry.repeatInterval : 2f;
        float shotTime = _nextRepeatIndex * repeatInterval;
        float triggerTime = Mathf.Max(0f, shotTime - entry.releaseTime / speed);

        if (_activeClock < triggerTime)
            return;

        Fire(entry);
        _nextRepeatIndex++;
    }

    private void Fire(AttackAnimationEntry entry)
    {
        _fired = true;

        if (entry == null || string.IsNullOrEmpty(entry.trigger))
            return;

        float speed = entry.speed > 0f ? entry.speed : 1f;
        if (_hasAttackSpeedParameter)
            _animator.SetFloat(AttackSpeedHash, speed);

        if (_networkAnimator != null)
            _networkAnimator.SetTrigger(entry.trigger);
        else
            _animator.SetTrigger(entry.trigger);
    }

    private void CacheFloatParameters()
    {
        if (_animator == null)
            return;

        AnimatorControllerParameter[] parameters = _animator.parameters;
        foreach (AnimatorControllerParameter parameter in parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Float)
                continue;

            if (parameter.nameHash == LocomotionHash)
                _hasLocomotionParameter = true;
            else if (parameter.nameHash == LocomotionSpeedHash)
                _hasLocomotionSpeedParameter = true;
            else if (parameter.nameHash == AttackSpeedHash)
                _hasAttackSpeedParameter = true;
        }
    }

    private AttackAnimationEntry FindEntry(ScriptableAttack attack)
    {
        if (attack == null || !_entriesByType.TryGetValue(attack.GetType(), out AttackAnimationEntry entry))
            return null;

        return entry;
    }
}

[System.Serializable]
public class AttackAnimationEntry
{
    public ScriptableAttack attack;
    public string trigger;
    public TriggerMoment moment;
    [Range(0.25f, 2f)] public float speed = 1f;
    public bool repeatWhileActive;
    [Range(0.25f, 5f)] public float repeatInterval = 2f;
    [Range(0f, 2f)] public float releaseTime = 0.8f;
}

public enum TriggerMoment
{
    OnStart,
    OnArrive,
    OnEnd
}
