using LordBreakerX.Utilities.Tags;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
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
    private float _damage = 50;

    private Vector3 _moveDirection = Vector3.zero;

    private Timer _timer;

    private GameObject _creator;

    private void Awake()
    {
        _timer = new Timer(_survivalTime);
    }

    private void OnEnable()
    {
        _timer.onTimerFinished += OnDeath;
    }

    private void OnDisable()
    {
        _timer.onTimerFinished -= OnDeath;
    }

    private void Update()
    {
        _timer.Step();

        transform.position += _moveDirection * _moveSpeed * Time.deltaTime;
    }

    private void OnDeath()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(_monsterTag) && IsServer)
        {
            dealDamage damage = collision.gameObject.GetComponent<dealDamage>();
            if (damage != null) damage.dealDamage(_damage, Color.red, _creator);
        }

        Destroy(gameObject);
    }

    public static void CreateLaser(Laser prefab, GameObject creator, Vector3 startPosition, Vector3 targetPosition)
    {
        Laser createdLaser = Instantiate(prefab, startPosition, Quaternion.identity);
        createdLaser._creator = creator;
        createdLaser._moveDirection = (targetPosition - startPosition).normalized;
    }
}
