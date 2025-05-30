using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Stomp")]
public class StompAttack : MonsterAttack
{
    [SerializeField]
    [Header("Properties")]
    [Min(0)]
    private float _maxStompDistance = 1.2f;

    [SerializeField]
    [Min(0)]
    private float _effectRadius = 1;

    private bool _finishedAttack = false;

    public override void OnInilization()
    {
        base.OnInilization();
    }

    public override void OnUpdate()
    {
        Vector3 attackPosition = GetAttackPosition();
        Vector3 checkPosition = new Vector3(attackPosition.x, Parent.transform.position.y, attackPosition.z);
        Monster.ChangeDestination(attackPosition);

        if (Vector3.Distance(Parent.transform.position, checkPosition) <= _maxStompDistance)
        {
            Monster.StopMovement();
            Monster.Stomp(_effectRadius);
            _finishedAttack = true;
        }
    }

    public override bool CanFinishAttack()
    {
        return _finishedAttack;
    }

    public override void OnStop()
    {
        _finishedAttack = false;
        Monster.StopMovement();
    }
}
