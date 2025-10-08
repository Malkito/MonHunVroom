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

    public StompAttack(AttackController controller) : base(controller)
    {
        _monsterMovement = controller.GetComponent<MonsterMovementController>();
    }

    public override Attack Clone(AttackController attackController)
    {
        StompAttack stomp = new StompAttack(attackController);
        stomp._maxStompDistance = _maxStompDistance;
        stomp._monsterTag = _monsterTag;
        stomp._stompEffect = _stompEffect;
        return stomp;
    }

    public override void OnStart()
    {
        _monsterMovement.UpdateWalkAnimation(true);
        _monsterMovement.StopMovement();
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

    public override void OnAttackUpdate()
    {
        _monsterMovement.ChangeDestination(GetTargetPosition());

        Vector3 currentPosition = Controller.transform.position;

        if (_monsterMovement.ReachedDestination())
        {
            _monsterMovement.StopMovement();
            PreformStomp();
            _finishedAttack = true;
            _monsterMovement.UpdateWalkAnimation(false);
        }
    }

    public void PreformStomp()
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

}
