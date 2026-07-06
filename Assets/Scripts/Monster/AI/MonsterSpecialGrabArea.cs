using LordBreakerX.Attributes;
using LordBreakerX.States.Networked;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MonsterSpecialGrabArea : MonoBehaviour
{
    [SerializeField]
    [RequiredField]
    private MonsterAttackController _attackController;

    [SerializeField]
    [RequiredField]
    private NetworkStateMachine _stateMachine;

    [SerializeField]
    [RequiredField]
    private MonsterGrabState _specialGrabState;

    [SerializeField]
    [Min(0)]
    private float _timeUntilGrab = 5;

    [SerializeField]
    private Dictionary<Transform, float> _times = new Dictionary<Transform, float>();

    [TagDropdown]
    private string _playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (_times.ContainsKey(other.transform) || other.CompareTag(_playerTag))
        {
            _times[other.transform] = _timeUntilGrab;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Transform otherTransform = other.transform;

        if (!_times.ContainsKey(otherTransform)) return;

        _times[otherTransform] -= Time.deltaTime;

        if (_times[otherTransform] <= 0 && !_stateMachine.IsState(_specialGrabState))
        {
            _times[otherTransform] = _timeUntilGrab;

            _attackController.SetTarget(otherTransform);
            _stateMachine.RequestTransitionTo(_specialGrabState);
        }
    }
}
