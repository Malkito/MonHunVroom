using UnityEngine;

/*
 *  Make the following changes: 
 *  1. monster should move to a max shooting distance if father then the distance)
 *  2. if the monster isn't facing the target then they should make themselves face the target.
 */

[CreateAssetMenu(menuName = "Monster/Attacks/Laser Eyes")]
public class LaserEyesAttack : Attack
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

    private MonsterAttackController _monsterAttack;

    private MonsterMovementController _monsterMovement;

    protected override void OnInilization(GameObject controlledObject)
    {
        base.OnInilization(controlledObject);
        _monsterAttack = controlledObject.GetComponent<MonsterAttackController>();
        _monsterMovement = controlledObject.GetComponent<MonsterMovementController>();

        _attackTimer = new Timer(_attackRate);
        _attackTimer.OnTimerFinished += () => _monsterAttack.RequestShootLaser(_laser, TargetProvider.GetTargetPosiiton());
    }

    public override void OnStart()
    {
        _monsterMovement.StopMovement();
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
}
