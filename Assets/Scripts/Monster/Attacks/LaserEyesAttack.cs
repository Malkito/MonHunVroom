using LordBreakerX.Utilities.Math;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Laser Eyes")]
public class LaserEyesAttack : MonsterAttack
{
    [SerializeField]
    [Header("Timing")]
    [Min(0f)]
    private float _duration;

    [SerializeField]
    [Min(0)]
    private float _attackRate = 0.5f;

    [SerializeField]
    [Header("Prefabs")]
    private Laser _laser;

    private float _durationLeft;

    private Timer _attackTimer;

    protected override void OnInilization()
    {
        base.OnInilization();
        _attackTimer = new Timer(_attackRate);
        _attackTimer.OnTimerFinished += () => Monster.RequestShootLaser(_laser, GetAttackPosition());
    }

    public override void OnStart()
    {
        Monster.StopMovement();
        _durationLeft = _duration;
    }

    public override void OnStop()
    {
        _attackTimer.Reset();
    }

    public override void OnUpdate()
    {
        _attackTimer.Update();
        _durationLeft -= Time.deltaTime;
    }

    public override bool CanFinishAttack()
    {
        return _durationLeft <= 0;
    }

    public override Vector3 GetRandomPosition()
    {
        Vector3 monsterOrigin = Monster.transform.position;
        Vector3 randomPosition = PositionUtility.GetRandomPositionInFrontHalfSquare(RandomPositionRange, monsterOrigin, Monster.transform.forward, Monster.transform.right);
        return randomPosition;
    }
}
