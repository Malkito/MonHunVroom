using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using UnityEngine;

[System.Serializable]
public class StompAttack : Attack
{
    private static readonly Color STOMP_FLASH_COLOR = Color.red;

    private const string STARTING_MONSTER_TAG = "Monster";

    [SerializeField]
    [Header("Properties")]
    [Min(0)]
    private float _maxStompDistance = 1.2f;

    [SerializeField]
    [TagDropdown]
    private string _monsterTag = STARTING_MONSTER_TAG;

    [SerializeField]
    private PrefabInstance<ParticleSystem> _stompEffect;

    private bool _finishedAttack;

    private MonsterMovementController _monsterMovement;

    public StompAttack(AttackController controller, StompAttack stomp) : base(controller)
    {
        _maxStompDistance = stomp._maxStompDistance;
        _monsterTag = stomp._monsterTag;
        _stompEffect = stomp._stompEffect;
        _finishedAttack = false;
        _monsterMovement = controller.GetComponent<MonsterMovementController>();
    }

    public override void OnStart()
    {
        _monsterMovement.UpdateWalkAnimation(true);
    }

    public override void OnStop()
    {
        _finishedAttack = false;
        _monsterMovement.StopMovement();
    }

    public override bool HasAttackFinished()
    {
        return _finishedAttack;
    }

    public void Stomp()
    {
        _stompEffect.GetOrCreateInstance(Controller.transform.position, Controller.transform).Play();

        Collider[] hits = Physics.OverlapSphere(Controller.transform.position, EnemyStatManager.StompRadius);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(_monsterTag))
            {
                dealDamage damage = hit.gameObject.GetComponent<dealDamage>();
                if (damage != null) 
                    damage.dealDamage(EnemyStatManager.StompDamage, STOMP_FLASH_COLOR, Controller.gameObject);
            }
        }
    }

    public override void OnAttackUpdate()
    {
        Vector3 targetPosition = OffsettedTargetPosition;

        _monsterMovement.ChangeDestination(targetPosition);

        Vector3 checkTargetPosition = new Vector3(targetPosition.x, Controller.transform.position.y, targetPosition.z);

        if (Vector3.Distance(Controller.transform.position, checkTargetPosition) <= _maxStompDistance)
        {
            _monsterMovement.StopMovement();
            Stomp();
            _finishedAttack = true;
            _monsterMovement.UpdateWalkAnimation(false);
        }
    }

    public override void OnAttackFixedUpdate() { }

}
