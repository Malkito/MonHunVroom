using LordBreakerX.AttackSystem;
using UnityEngine;

[System.Serializable]
public class BlackholeAttack : Attack
{
    [SerializeField]
    private BlackholeController _prefab;

    [SerializeField]
    private Vector3 _spawnOffset = Vector3.up;

    private BlackholeController _currentBlackhole;

    private MonsterMovementController _monsterMovement;

    public BlackholeAttack(AttackController controller) : base(controller)
    {
        _monsterMovement = controller.GetComponent<MonsterMovementController>();
    }

    public override Attack Clone(AttackController attackController)
    {
        BlackholeAttack attack = new BlackholeAttack(attackController);
        attack._prefab = _prefab;
        attack._spawnOffset = _spawnOffset;
        return attack;
    }

    public override void OnStart()
    {
        if (_currentBlackhole == null)
        {
            Vector3 position = Controller.transform.position + _spawnOffset;
            _currentBlackhole = _prefab.Clone(position);
        }
    }

    public override void OnStop()
    {
        _monsterMovement.StopMovement();
    }

    public override bool HasAttackFinished()
    {
        return _currentBlackhole == null;
    }

    public override void OnAttackUpdate()
    {
        _monsterMovement.Wander();
    }
}
