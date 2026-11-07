using System.Collections.Generic;
using LordBreakerX.AttackSystem;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAttackAnimator : MonoBehaviour
{
    [SerializeField] private AttackController _attackController;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Animator _animator;
    [SerializeField] private NetworkAnimator _networkAnimator;
    [SerializeField] private float _arriveDistance = 1f;
    [SerializeField] private AttackAnimationEntry[] _entries;

    private Dictionary<System.Type, AttackAnimationEntry> _entriesByType;
    private AttackAnimationEntry _currentEntry;
    private bool _fired;
    private int _framesSinceStart;

    private void Awake()
    {
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

        if (_currentEntry != null && _currentEntry.moment == TriggerMoment.OnStart)
            Fire(_currentEntry);
    }

    private void HandleStopped(ScriptableAttack attack)
    {
        if (_currentEntry != null && _currentEntry.moment == TriggerMoment.OnEnd && !_fired)
            Fire(_currentEntry);

        _currentEntry = null;
    }

    private void Update()
    {
        if (!_attackController.IsServer || _currentEntry == null || _currentEntry.moment != TriggerMoment.OnArrive || _fired)
            return;

        if (_framesSinceStart >= 1 && !_agent.pathPending && _agent.remainingDistance <= _arriveDistance)
            Fire(_currentEntry);

        _framesSinceStart++;
    }

    private void Fire(AttackAnimationEntry entry)
    {
        _fired = true;

        if (entry == null || string.IsNullOrEmpty(entry.trigger))
            return;

        if (_networkAnimator != null)
            _networkAnimator.SetTrigger(entry.trigger);
        else
            _animator.SetTrigger(entry.trigger);
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
}

public enum TriggerMoment
{
    OnStart,
    OnArrive,
    OnEnd
}
