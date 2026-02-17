using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Attacks/Stomp Attack")]
public class StompAttack : ScriptableAttack
{
    private static readonly Color STOMP_FLASH_COLOR = Color.red;

    private const string STARTING_MONSTER_TAG = "Monster";

    [Header("Physics")]
    [Min(0)]
    [SerializeField]
    private float _stompPushForce = 500f;

    [SerializeField]
    [Min(0)]
    private float _maxStompDistance = 1.2f;

    [Header("Tags")]
    [SerializeField]
    [TagDropdown]
    private string _monsterTag = STARTING_MONSTER_TAG;

    [Header("Effects")]
    [SerializeField]
    private ParticleInstance _stompEffect;

    private MonsterMovementController _monsterMovement;

    private MonsterAttackController _monsterAttack;

    public override void OnAttackCreation()
    {
        _monsterMovement = Controller.GetComponent<MonsterMovementController>();
        _monsterAttack = Controller.GetComponent<MonsterAttackController>();
        
    }

    public override void OnAttackStarted()
    {
        _monsterMovement.UpdateWalkAnimation(true);
    }

    public override void OnAttackUpdate()
    {
        _monsterMovement.ChangeDestination(Target.GetPosition());

        Vector3 targetPosition = Target.GetPosition();

        if (_monsterMovement.ReachedDestination(targetPosition, EnemyStatManager.StompRadius))
        {
            _monsterMovement.StopMovement();
            _monsterMovement.UpdateWalkAnimation(false);

            _stompEffect.GetOrCreateInstance(Controller.transform).Play();
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
                Vector3 direction = (hit.transform.position - Position).normalized;
                hit.attachedRigidbody.AddForce(_stompPushForce * hit.attachedRigidbody.mass * direction, ForceMode.Force);
            }
        }
    }

    public override void OnAttackStopped()
    {
        _monsterMovement.StopMovement();
    }

    public override bool HasAttackFinished()
    {
        return _monsterMovement.ReachedDestination();
    }

}
