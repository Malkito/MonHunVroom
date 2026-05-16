using LordBreakerX.AttackSystem;
using LordBreakerX.Stats;
using UnityEngine;

[CreateAssetMenu()]
public sealed class BlackholeAttack : ScriptableAttack
{
    [SerializeField]
    private BlackholeController _prefab;

    [SerializeField]
    private Vector3 _spawnOffset = Vector3.up;

    [SerializeField]
    private float _maxAttackDistance;

    private BlackholeController _currentBlackhole;
    private MonsterMovementController _monsterMovement;

    private bool _spawnedBlackhole = false;

    private float _attackDamage;

    private StatHolder _statHolder;

    public override void OnAttackCreation()
    {
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
        _statHolder = Controller.GetComponent<StatHolder>();
    }

    public override void OnAttackStarted()
    {
        _spawnedBlackhole = false;
        _currentBlackhole = null;

        _attackDamage = _statHolder.GetFloat("Blackhole-Damage");
    }

    public override void OnAttackStopped()
    {
        _monsterMovement.StopMovement();
    }

    public override bool HasAttackFinished()
    {
        return _spawnedBlackhole;
    }

    public override bool CanUseAttack()
    {
        return _currentBlackhole == null;
    }

    public override void OnAttackUpdate()
    {
        if (!_spawnedBlackhole)
        {
            Vector3 targetPosition = Target.GetPosition();
            _monsterMovement.ChangeDestination(targetPosition);

            if (_monsterMovement.ReachedDestination(targetPosition, _maxAttackDistance))
            {
                Vector3 position = Controller.transform.position + _spawnOffset;
                _currentBlackhole = _prefab.Clone(_attackDamage, position);
                Controller.SpawnProjectile(_currentBlackhole.gameObject);
                _spawnedBlackhole = true;
            }
        }
    }
}
