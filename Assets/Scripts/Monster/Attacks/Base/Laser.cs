using LordBreakerX.Attributes;
using Unity.Netcode;
using UnityEngine;

public class Laser : NetworkBehaviour
{
    [SerializeField]
    [Min(0)]
    private float _survivalTime = 10;

    [SerializeField]
    [Min(0)]
    private float _moveSpeed;

    [SerializeField]
    [TagDropdown]
    private string _monsterTag = "Monster";

    [SerializeField]
    ParticleSystem _hitParticle;

    private Vector3 _moveDirection = Vector3.zero;

    private Timer _timer;

    private GameObject _creator;

    private MonsterStatManager _statManager;

    private void Awake()
    {
        _timer = new Timer(_survivalTime);
    }

    private void OnEnable()
    {
        _timer.OnTimerFinished += OnDeath;
    }

    private void OnDisable()
    {
        _timer.OnTimerFinished -= OnDeath;
    }

    private void Update()
    {
        _timer.Update();

        transform.position += _moveDirection * _statManager.LaserEyeSpeed * Time.deltaTime;
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(_monsterTag))
        {
            dealDamage damage = collision.gameObject.GetComponent<dealDamage>();
            if (damage != null) damage.dealDamage(_statManager.LaserEyesDamage, Color.red, _creator);
        }

        if (_hitParticle != null) _hitParticle.Play();
        Destroy(gameObject, 1);
    }

    public static void CreateLaser(Laser prefab, GameObject creator, Vector3 startPosition, Vector3 targetPosition, MonsterStatManager statManager)
    {
        Laser createdLaser = Instantiate(prefab, startPosition, Quaternion.identity);
        createdLaser._creator = creator;
        createdLaser._moveDirection = (targetPosition - startPosition).normalized;
        createdLaser.transform.LookAt(targetPosition, Vector3.up);
        createdLaser._statManager = statManager;
    }
}
