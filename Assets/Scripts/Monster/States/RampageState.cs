using LordBreakerX.Attributes;
using LordBreakerX.States;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = MonsterStates.RAMPAGE, menuName = MonsterStates.CreatePaths.RAMPAGE)]
public class RampageState : BaseState
{
    [SerializeField]
    [Min(0)]
    private float _damageableTargetingChance = 45.0f;

    [SerializeField]
    [Min(0)]
    private float _attackRadius = 30.0f;

    [SerializeField]
    [TagDropdown]
    private string _monsterTag = "Monster";

    private MonsterMovementController _monsterMovement;
    private MonsterAttackController _monsterAttack;

    public override string ID => MonsterStates.RAMPAGE;

    private Dictionary<Collider, dealDamage> _damageables = new Dictionary<Collider, dealDamage>();

    private void OnValidate()
    {
        _damageableTargetingChance = Mathf.Clamp(_damageableTargetingChance, 0.0f, 100.0f);
    }

    protected override void OnInitilization()
    {
        _monsterMovement = StateObject.GetComponent<MonsterMovementController>();
        _monsterAttack = StateObject.GetComponent<MonsterAttackController>();
    }

    public override void Enter()
    {
        _monsterAttack.RequestRandomAttack();
    }

    public override void Exit()
    {
        _monsterAttack.RequestStopAttack();
        _monsterMovement.StopMovement();
    }

    public override void Update()
    {
        _monsterAttack.PlayerAttackTimer.Update();

        if (!_monsterAttack.IsAttacking && !_monsterAttack.RequestingAttack)
        {
            Machine.RequestChangeState(MonsterStates.WANDER);
        }
    }

    public override void OnGizmosSelected()
    {
        Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.1f);
        Gizmos.DrawSphere(StateObject.transform.position, _attackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(StateObject.transform.position, _attackRadius);
    }
}
