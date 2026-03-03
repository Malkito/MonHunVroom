using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Bug Attack")]
public sealed class BugAttack : ScriptableAttack
{
    [SerializeField]
    [Min(0f)]
    private float _maxSpawnDistance = 30f;

    [Header("Sworm Properties")]
    [SerializeField]
    [Min(0)]
    private float _yOffset = 5;

    [SerializeField]
    [Min(0)]
    private float _spawnRadius = 5;

    [SerializeField]
    [Min(1)]
    private int _minSpawns = 1;

    [SerializeField]
    [Min(1)]
    private int _maxSpawns = 1;

    [Header("Random Target Properties")]
    [SerializeField]
    [Min(0)]
    private float _randomTargetChance = 40.0f;

    [SerializeField]
    [Min(0)]
    private float _targetRadius = 40;

    [SerializeField]
    [Min(1)]
    private int _swormLimit;

    [Header("Prefab Properties")]
    [SerializeField]
    private BugSworm _swormPrefab;

    private bool _spawnedSworms = false;

    private MonsterMovementController _monsterMovement;

    public override void OnAttackCreation()
    {
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
    }

    public override void OnAttackStarted()
    {
        _spawnedSworms = false;
    }

    public override bool HasAttackFinished()
    {
        return _spawnedSworms;
    }

    public override void OnAttackUpdate()
    {
        if (!_spawnedSworms)
        {
            Vector3 targetPosition = Target.GetPosition();
            _monsterMovement.ChangeDestination(targetPosition);

            if (_monsterMovement.ReachedDestination(targetPosition, _maxSpawnDistance))
            {
                SpawnSworms();
            }
        }
    }

    private void SpawnSworms()
    {
        int swormAmount = Mathf.Clamp(Random.Range(_minSpawns, _maxSpawns), 0, _swormLimit - BugSworm.TotalSworms);

        for (int i = 0; i < swormAmount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            BugSworm sworm = null;

            if (Probability.IsSuccessful(_randomTargetChance))
            {
                AttackTarget randomTarget = TargetUtility.GetRandomTarget<dealDamage>(Position, _targetRadius, Controller.IgnoredLayers);
                sworm = BugSworm.SpawnSworm(_swormPrefab, spawnPosition, randomTarget);
            }
            else
            {
                sworm = BugSworm.SpawnSworm(_swormPrefab, spawnPosition, Target);
            }
            Controller.SpawnProjectile(sworm.gameObject);
        }

        _spawnedSworms = true;
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 position = Controller.transform.position + (Random.onUnitSphere * _spawnRadius);
        position.y += _yOffset;
        return position;
    }

    public override bool CanUseAttack()
    {
        return BugSworm.TotalSworms < _swormLimit;
    }

}
