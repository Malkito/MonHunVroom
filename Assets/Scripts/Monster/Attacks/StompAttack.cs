using LordBreakerX.Attributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/Attacks/Stomp")]
public class StompAttack : Attack
{
    private const float STOMP_DAMAGE_AMOUNT = 50;

    private static readonly Color STOMP_FLASH_COLOR = Color.red;

    private const string STARTING_MONSTER_TAG = "Monster";

    [SerializeField]
    [Header("Properties")]
    [Min(0)]
    private float _maxStompDistance = 1.2f;

    [SerializeField]
    [Min(0)]
    private float _effectRadius = 1;

    [SerializeField]
    [TagDropdown]
    private string _monsterTag = STARTING_MONSTER_TAG;

    [SerializeField]
    private PrefabInstance<ParticleSystem> _stompEffect;

    private bool _finishedAttack = false;

    private MonsterMovementController _movementController;

    protected override void OnInilization(GameObject controlledObject)
    {
        _movementController = controlledObject.GetComponent<MonsterMovementController>();
    }

    public override void OnStart()
    {
        _movementController.UpdateWalkAnimation(true);
    }

    public override void OnUpdate()
    {
        _movementController.ChangeDestination(OffsettedTargetPosition);

        if (Vector3.Distance(AttackHandler.transform.position, OffsettedTargetPosition) <= _maxStompDistance)
        {
            _movementController.StopMovement();
            Stomp();
            _finishedAttack = true;
            _movementController.UpdateWalkAnimation(false);
        }
    }

    public override bool CanFinishAttack()
    {
        return _finishedAttack;
    }

    public override void OnStop()
    {
        _finishedAttack = false;
        _movementController.StopMovement();
    }

    public void Stomp()
    {
        _stompEffect.GetOrCreateInstance(AttackHandler.transform.position, AttackHandler.transform).Play();

        Collider[] hits = Physics.OverlapSphere(AttackHandler.transform.position, _effectRadius);

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag(_monsterTag))
            {
                dealDamage damage = hit.gameObject.GetComponent<dealDamage>();
                if (damage != null) damage.dealDamage(STOMP_DAMAGE_AMOUNT, STOMP_FLASH_COLOR, AttackHandler.gameObject);
            }
        }
    }
}
