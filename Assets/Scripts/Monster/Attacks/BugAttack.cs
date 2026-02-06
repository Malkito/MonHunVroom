using LordBreakerX.AttackSystem;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BugAttack : Attack
{
    [Header("Sworm Properties")]
    [SerializeField]
    [Min(0)]
    private float _yOffset = 5;

    [SerializeField]
    [Min(0)]
    private float _spawnRadius = 5;

    [SerializeField]
    [Min(0f)]
    private float _attackDuration = 10;

    [SerializeField]
    [Min(1)]
    private int _minSworms = 1;

    [SerializeField]
    [Min(1)]
    private int _maxSworms = 1;

    [Header("Prefab Properties")]
    [SerializeField]
    private BugSworm _swormPrefab;

    private List<BugSworm> _activeSworms = new List<BugSworm>();
    private float _durationLeft;

    public BugAttack(AttackController controller) : base(controller)
    {
    }

    public override void OnStart()
    {
        _durationLeft = _attackDuration;

        int swormAmount = Random.Range(_minSworms, _maxSworms);

        for (int i = 0; i < swormAmount; i++) 
        {
            Vector3 spawnPosition = GetSpawnPosition();

            BugSworm sworm = BugSworm.SpawnSworm(_swormPrefab, spawnPosition, Controller.Target);
            _activeSworms.Add(sworm);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 position = Controller.transform.position + (Random.onUnitSphere * _spawnRadius);
        position.y += _yOffset;
        return position;
    }

    public override void OnAttackUpdate()
    {
        Vector3 targetPosition = GetTargetPosition();

        _durationLeft -= Time.deltaTime;
        //foreach (BugSworm sworm in _activeSworms) 
        //{
        //    sworm.transform.position = Vector3.MoveTowards(sworm.transform.position, targetPosition, sworm.Speed * Time.deltaTime);
        //}
    }

    public override void OnStop()
    {
        foreach(BugSworm sworm in _activeSworms)
        {
            if (sworm != null) 
                GameObject.Destroy(sworm.gameObject);
        }
        _activeSworms.Clear();
    }

    public override bool HasAttackFinished()
    {
        return _durationLeft <= 0;
    }

    public override Attack Clone(AttackController controller)
    {
        BugAttack attackInstance = new BugAttack(controller);
        attackInstance._spawnRadius = _spawnRadius;
        attackInstance._swormPrefab = _swormPrefab;
        attackInstance._attackDuration = _attackDuration;
        attackInstance._minSworms = _minSworms;
        attackInstance._maxSworms = _maxSworms;
        return attackInstance;
    }
}
