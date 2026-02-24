using LordBreakerX.AttackSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Earth Throw Attack")]
public sealed class EarthThrow : ScriptableAttack
{
    [SerializeField]
    private float _maxThrowDistance = 50;

    [SerializeField]
    private Roubble _roublePrefab;

    private MonsterMovementController _monsterMovement;

    private bool _thrownEarth = false;

    public override void OnAttackCreation()
    {
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
    }

    public override void OnAttackStarted()
    {
        _thrownEarth = false;
    }

    public override bool HasAttackFinished()
    {
        return _thrownEarth;
    }

    public override void OnAttackUpdate()
    {
        Vector3 targetPosition = Target.GetPosition();

        if (_monsterMovement.ReachedDestination(targetPosition, _maxThrowDistance))
        {
            _monsterMovement.StopMovement();
            Roubble roubble = _roublePrefab.CreateRouble(Position, targetPosition);
            Controller.SpawnProjectile(roubble.gameObject);
            _thrownEarth = true;
        }
        else
        {
            _monsterMovement.ChangeDestination(targetPosition);
        }
    }
}
