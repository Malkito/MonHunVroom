using LordBreakerX.AttackSystem;
using UnityEngine;

[System.Serializable]
public class BlackholeAttack : Attack
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

    public BlackholeAttack(AttackController controller) : base(controller)
    {
        _monsterMovement = controller.GetComponent<MonsterMovementController>();
    }

    public override Attack Clone(AttackController attackController)
    {
        BlackholeAttack attack = new BlackholeAttack(attackController);
        attack._prefab = _prefab;
        attack._spawnOffset = _spawnOffset;
        attack._maxAttackDistance = _maxAttackDistance;
        return attack;
    }

    public override void OnStart()
    {
        _spawnedBlackhole = false;
        _currentBlackhole = null;
    }

    public override void OnStop()
    {
        _monsterMovement.StopMovement();
    }

    public override bool HasAttackFinished()
    {
        return _currentBlackhole == null && _spawnedBlackhole;
    }

    public override void OnAttackUpdate()
    {
        if (!_spawnedBlackhole)
        {
            Vector3 targetPosition = GetTargetPosition();
            _monsterMovement.ChangeDestination(targetPosition);

            if (_monsterMovement.ReachedDestination(_maxAttackDistance))
            {
                Vector3 position = Controller.transform.position + _spawnOffset;
                _currentBlackhole = _prefab.Clone(position);
                _spawnedBlackhole = true;
            }
        }
        else
        {
            _monsterMovement.Wander();
        }
    }
}
