using LordBreakerX.Attributes;
using LordBreakerX.States;
using LordBreakerX.Utilities.AI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
        StartRandomAttack();
    }

    public override void Update()
    {
        _monsterAttack.PlayerAttackTimer.Update();

        if (!_monsterAttack.IsAttacking && !_monsterAttack.IsRequestingAttack)
        {
            Machine.RequestChangeState(MonsterStates.WANDER);
        }
    }

    private void StartRandomAttack()
    {
        Vector3 start = StateObject.transform.position;
        float chance = Random.Range(0.0f, 100.0f);

        if (chance <= _damageableTargetingChance && TryTargetDamageable()) return;

        _monsterAttack.RequestAttackRandomPosition(StateObject.transform.position, _attackRadius);
    }

    private bool TryTargetDamageable()
    {
        List<Collider> validDamageables = new List<Collider>();

        Collider[] overlapColliders = Physics.OverlapSphere(StateObject.transform.position, _attackRadius);

        NavMeshPath path = new NavMeshPath();

        foreach (Collider protentialDamageable in overlapColliders)
        {
            if (IsValidDamageable(protentialDamageable, path))
            {
                validDamageables.Add(protentialDamageable);
            }
        }
        return ChooseRandomTarget(validDamageables);
    }

    private bool ChooseRandomTarget(List<Collider> elements)
    {
        if (elements.Count > 0)
        {
            int randomIndex = Random.Range(0, elements.Count);
            _monsterAttack.TargetProvider.SetTarget(elements[randomIndex].transform, StateObject.transform.position);
            _monsterAttack.RequestStartAttack();
            return true;
        }

        return false;
    }

    private bool IsValidDamageable(Collider collider, NavMeshPath path)
    {
        if (!_damageables.ContainsKey(collider))
        {
            dealDamage damageable = collider.GetComponent<dealDamage>();
            _damageables.Add(collider, damageable);
        }
        return _damageables[collider] != null && !collider.CompareTag(_monsterTag) && _monsterAttack.TargetProvider.IsValidTarget(collider.transform, StateObject.transform.position);
    }

    public override void OnGizmosSelected()
    {
        Gizmos.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.1f);
        Gizmos.DrawSphere(StateObject.transform.position, _attackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(StateObject.transform.position, _attackRadius);
    }
}
