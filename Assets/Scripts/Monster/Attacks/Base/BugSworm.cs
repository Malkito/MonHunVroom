using LordBreakerX.AttackSystem;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BugSworm : NetworkBehaviour
{
    [Header("Movement Properties")]
    [SerializeField]
    [Min(0f)]
    private float _minMoveSpeed = 5;

    [SerializeField]
    [Min(0f)]
    private float _maxMoveSpeed = 10.0f;

    [SerializeField]
    [Min(0f)]
    private float _targetOffsetRadius = 10.0f;

    [Header("Damage Properties")]
    [SerializeField]
    [Min(0f)]
    private float _damage = 1.0f;

    [SerializeField]
    [Min(0f)]
    private float _timeBetweenDamage = 2.0f;

    [Header("Bug Properties")]
    [SerializeField]
    [Range(1, 50)]
    private int _minBugs = 3;

    [SerializeField]
    [Range(1, 50)]
    private int _maxBugs = 5;

    [Header("Prefabs Properties")]
    [SerializeField]
    private Bug _bugPrefab = null;

    public float Speed { get; private set; }

    private float _durationLeft = 0f;

    private List<dealDamage> _attackTargets = new List<dealDamage>();

    private Vector3 _targetOffset;
    private AttackTarget _target;

    private void OnValidate()
    {
        _maxBugs = Mathf.Max(_minBugs, _maxBugs);
        _maxMoveSpeed = Mathf.Max(_minMoveSpeed, _maxMoveSpeed);
    }

    private void Awake()
    {
        Speed = Random.Range(_minMoveSpeed, _maxMoveSpeed);

        _durationLeft -= _timeBetweenDamage;
        int bugsAmount = Random.Range(_minBugs, _maxBugs);

        for (int i = 0; i < bugsAmount; i++) 
        {
            Bug.SpawnBug(_bugPrefab, transform);
        }
    }

    private void Update()
    {
        _durationLeft -= Time.deltaTime;

        if (_durationLeft <= 0f)
        {
            foreach (var target in _attackTargets) 
            {
                target.dealDamage(_damage, Color.red, gameObject);
            }

            _durationLeft = _timeBetweenDamage;
        }


        Vector3 targetPosition = _target.GetCenteredTargetPosition() + _targetOffset;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);

        if (AtTarget())
        {
            UpdateTargetOffset();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<bullet>() != null)
        {
            Destroy(gameObject);
        }


        dealDamage damageable = other.GetComponent<dealDamage>();
        if (damageable != null && !_attackTargets.Contains(damageable))
        {
            _attackTargets.Add(damageable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        dealDamage damageable = other.GetComponent<dealDamage>();
        if (damageable != null && _attackTargets.Contains(damageable))
        {
            _attackTargets.Remove(damageable);
        }
    }

    public static BugSworm SpawnSworm(BugSworm swormPrefab, Vector3 spawnPosition, AttackTarget target)
    {
        BugSworm sworm = Instantiate(swormPrefab, spawnPosition, Quaternion.identity);
        sworm._target = target;
        sworm.UpdateTargetOffset();
        return sworm;
    }

    private void UpdateTargetOffset()
    {
        _targetOffset = Random.insideUnitSphere * _targetOffsetRadius;
        _targetOffset.y = 0;
    }

    private bool AtTarget(float maxDistance = 0.2f)
    {
        Vector3 targetPosition = _target.GetCenteredTargetPosition() + _targetOffset;
        return Vector3.Distance(transform.position, targetPosition) <= maxDistance;
    }
}
