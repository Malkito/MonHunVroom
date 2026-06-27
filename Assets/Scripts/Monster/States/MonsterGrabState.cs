using UnityEngine;

/// <summary>
/// Special state started when a player enters above the monster for a short period
/// </summary>
[CreateAssetMenu(menuName = MonsterState.CREATE_PATH + "Special Grab State")]
public class MonsterGrabState : MonsterState
{
    [SerializeField]
    private SpecialGrabAttack _specialGrab;

    private bool _startedAttack;

    [SerializeField]
    private MonsterWanderState _wanderState;

    protected override void OnEnterState()
    {
        AttackHandler.StopAttack();
        _startedAttack = false;
    }

    protected override void OnUpdateState()
    {
        if (AttackHandler.IsAttacking) return;

        if (_startedAttack)
        {
            Machine.RequestTransitionTo(_wanderState);
        }
        else
        {
            AttackHandler.StartEntryAttack(_specialGrab);
            _startedAttack = true;
        }
    }

    
}
