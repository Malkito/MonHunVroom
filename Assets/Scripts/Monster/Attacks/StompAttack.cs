using LordBreakerX.AttackSystem;
using LordBreakerX.Attributes;
using UnityEngine;

//[CreateAssetMenu(menuName = "Monster/Attacks/Stomp")]
//public class StompAttack : ScriptableAttack
//{
//    private static readonly Color STOMP_FLASH_COLOR = Color.red;

//    private const string STARTING_MONSTER_TAG = "Monster";

//    [SerializeField]
//    [Header("Properties")]
//    [Min(0)]
//    private float _maxStompDistance = 1.2f;

//    [SerializeField]
//    [TagDropdown]
//    private string _monsterTag = STARTING_MONSTER_TAG;

//    [SerializeField]
//    private PrefabInstance<ParticleSystem> _stompEffect;

//    public override void OnUpdate()
//    {
//        Vector3 targetPosition = OffsettedTargetPosition;

//        _movementController.ChangeDestination(targetPosition);

//        Vector3 checkTargetPosition = new Vector3(targetPosition.x, AttackHandler.transform.position.y, targetPosition.z);

//        if (Vector3.Distance(AttackHandler.transform.position, checkTargetPosition) <= _maxStompDistance)
//        {
//            _movementController.StopMovement();
//            Stomp();
//            _finishedAttack = true;
//            _movementController.UpdateWalkAnimation(false);
//        }
//    }

//    public override bool CanFinishAttack()
//    {
//        return _finishedAttack;
//    }

//    public override void OnStop()
//    {
//        _finishedAttack = false;
//        _movementController.StopMovement();
//    }

//    public void Stomp()
//    {
//        _stompEffect.GetOrCreateInstance(AttackHandler.transform.position, AttackHandler.transform).Play();

//        Collider[] hits = Physics.OverlapSphere(AttackHandler.transform.position, EnemyStatManager.StompRadius);

//        foreach (Collider hit in hits)
//        {
//            if (!hit.CompareTag(_monsterTag))
//            {
//                dealDamage damage = hit.gameObject.GetComponent<dealDamage>();
//                if (damage != null) damage.dealDamage(EnemyStatManager.StompDamage, STOMP_FLASH_COLOR, AttackHandler.gameObject);
//            }
//        }
//    }

//    public override void OnStart(AttackController attackHandler)
//    {
//        attackHandler.Movement.UpdateWalkAnimation(true);
//    }

//    public override void OnStop(AttackController attackHandler)
//    {

//    }

//    public override void OnAttackFixedUpdate(AttackController attackHandler)
//    {
//        throw new System.NotImplementedException();
//    }

//    public override void OnAttackUpdate(AttackController attackHandler)
//    {
//        throw new System.NotImplementedException();
//    }

//    public override void OnPreperationFixedUpdate(AttackController attackHandler)
//    {
//        throw new System.NotImplementedException();
//    }

//    public override void OnPreperationUpdate(AttackController attackHandler)
//    {
//        throw new System.NotImplementedException();
//    }

//    public override AttackProgress GetAttackProgress(AttackController attackHandler)
//    {
//        throw new System.NotImplementedException();
//    }
//}
