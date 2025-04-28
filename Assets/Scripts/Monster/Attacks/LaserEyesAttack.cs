using LordBreakerX.Utilities.Math;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Monster/Laser Eyes")]
public class LaserEyesAttack : MonsterAttackAbility
{
    [SerializeField]
    [Header("Timed Ability")]
    [Min(0f)]
    private float _duration;

    private float _durationLeft;

    [SerializeField]
    [Header("Properties")]
    private Laser _laser;

    [SerializeField]
    [Min(0)]
    private float _attackRate = 0.5f;

    private Timer _attackTimer;

    private Vector3 _targetPosition;

    public override void BeginAbility()
    {
        _durationLeft = _duration;
        _targetPosition = GetTargetPosition();
    }

    protected override void OnInitilization()
    {
        base.OnInitilization();
        _attackTimer = new Timer(_attackRate);
        _attackTimer.onTimerFinished += Attack;
    }

    private void Attack()
    {
        Transform eye = Monster.GetRandomEye();
        Laser.Create(_laser, Handler.gameObject, eye.position, _targetPosition);
    }

    public override void FinishAbility()
    {
        _attackTimer.Reset();
    }



    public override void FixedUpdate()
    {
        
    }

    public override void Update()
    {
        _attackTimer.Step();

        _durationLeft -= Time.deltaTime;

        if (_durationLeft <= 0)
        {
            Handler.StopAbility(ID);
        }
    }

    public override Vector3 RandomTargetPosition()
    {
        Vector3 monsterOrigin = Monster.transform.position + Monster.MonsterBottom;
        Vector3 randomPosition = PositionUtility.GetRandomPositionInFrontHalfSquare(TargetRange, monsterOrigin, Monster.transform.forward, -Monster.transform.right);
        return randomPosition;
    }
}
