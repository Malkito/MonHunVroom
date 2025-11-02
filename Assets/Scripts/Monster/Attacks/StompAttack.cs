using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using UnityEngine;

[System.Serializable]
public class StompAttack : Attack
{
    private static readonly Color STOMP_FLASH_COLOR = Color.red;

    private const string STARTING_MONSTER_TAG = "Monster";

    [SerializeField]
    [Header("Activation Requirements")]
    [Min(0)]
    private float _maxStompDistance = 1.2f;

    [Header("Tags")]
    [SerializeField]
    [TagDropdown]
    private string _monsterTag = STARTING_MONSTER_TAG;

    [SerializeField]
    private float _stompPushForce = 500f;

    private MonsterMovementController _monsterMovement;

    private MonsterAttackController _monsterAttack;


    public StompAttack(AttackController controller) : base(controller)
    {
        _monsterMovement = controller.GetComponent<MonsterMovementController>();
        _monsterAttack = controller.GetComponent<MonsterAttackController>();
    }

    public override Attack Clone(AttackController attackController)
    {
        StompAttack stomp = new StompAttack(attackController);
        stomp._maxStompDistance = _maxStompDistance;
        stomp._monsterTag = _monsterTag;
        return stomp;
    }

    public override void OnStart()
    {
        _monsterMovement.UpdateWalkAnimation(true);
    }

    public override void OnAttackUpdate()
    {
        _monsterMovement.ChangeDestination(GetTargetPosition());

        Vector3 currentPosition = Controller.transform.position;

        if (_monsterMovement.ReachedDestination(EnemyStatManager.StompRadius))
        {
            _monsterMovement.StopMovement();
            _monsterMovement.UpdateWalkAnimation(false);
            _monsterAttack.PlayEffect(MonsterAttackEffect.Stomp);
            PreformStomp();
        }
    }

    public void PreformStomp()
    {
        Collider[] attackHits = Physics.OverlapSphere(Controller.transform.position, EnemyStatManager.StompRadius);

        foreach (Collider hit in attackHits)
        {
            if (hit.CompareTag(_monsterTag)) continue;

            dealDamage damageable = hit.gameObject.GetComponent<dealDamage>();
            damageable?.dealDamage(EnemyStatManager.StompDamage, STOMP_FLASH_COLOR, Controller.gameObject);

            if (hit.attachedRigidbody != null) 
            {
                Vector3 direction = (hit.transform.position - GetStartPosition()).normalized;
                hit.attachedRigidbody.AddForce(_stompPushForce * hit.attachedRigidbody.mass * direction, ForceMode.Force);
            }
        }
    }

    public override void OnStop()
    {
        _monsterMovement.StopMovement();
    }

    public override bool CanUseAttack()
    {
        return _monsterMovement.ReachedDestination(_maxStompDistance);
    }

    public override bool HasAttackFinished()
    {
        return _monsterMovement.ReachedDestination();
    }

}
