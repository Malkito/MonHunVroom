using LordBreakerX.AbilitySystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Monster/Laser Eyes")]
public class LaserEyesAttack : TimedAbility
{
    [SerializeField]
    [Header("Properties")]
    private Laser _laser;

    [SerializeField]
    [Min(0)]
    private float _attackRate = 0.5f;

    [SerializeField]
    private MonsterAbilityUtility _utility = new MonsterAbilityUtility();

    private Timer _attackTimer;

    private Vector3 _targetPosition;

    protected override void OnInitilization()
    {
        _utility.Initilize(Handler);
        _attackTimer = new Timer(_attackRate);
        _attackTimer.onTimerFinished += Attack;
    }

    private void Attack()
    {
        Transform eye = _utility.Monster.GetRandomEye();
        Laser.Create(_laser, eye.position, _targetPosition);
    }

    protected override void ActivateAbility()
    {
        _targetPosition = _utility.GetTargetPosition();
    }

    public override bool CanUse()
    {
        return true;
    }

    public override void FinishAbility()
    {
        _attackTimer.Reset();
    }


    protected override void TimedUpdate()
    {
        _attackTimer.Step();
    }

    public override void FixedUpdate()
    {
        
    }
}
