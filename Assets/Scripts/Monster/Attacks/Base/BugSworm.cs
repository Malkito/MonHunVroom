using LordBreakerX.AttackSystem;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BugSworm : NetworkBehaviour
{
    public static int TotalSworms { get; private set; }

    [Header("Movement Properties")]
    [SerializeField]
    [Min(0f)]
    private float _minMoveSpeed = 5;

    [SerializeField]
    [Min(0f)]
    private float _maxMoveSpeed = 10.0f;

    [SerializeField]
    [Min(0f)]
    private float _targetOffsetRadius = 10.0f;

    [SerializeField]
    [Min(0)]
    private float _randomTargetRadius = 40;

    [SerializeField]
    private LayerMask _ignoredLayers;

    [Header("Damage Properties")]
    [SerializeField]
    [Min(0f)]
    private float _damagePerSecond = 1.0f;

    [Header("Bug Properties")]
    [SerializeField]
    [Range(1, 50)]
    private int _minBugs = 3;

    [SerializeField]
    [Range(1, 50)]
    private int _maxBugs = 5;

    [Header("Prefabs Properties")]
    [SerializeField]
    private Bug _bugPrefab = null;

    public float Speed { get; private set; }

    private Vector3 _targetOffset;
    private AttackTarget _target;

    private void OnValidate()
    {
        _maxBugs = Mathf.Max(_minBugs, _maxBugs);
        _maxMoveSpeed = Mathf.Max(_minMoveSpeed, _maxMoveSpeed);
    }

    private void Awake()
    {
        Speed = Random.Range(_minMoveSpeed, _maxMoveSpeed);

        int bugsAmount = Random.Range(_minBugs, _maxBugs);

        for (int i = 0; i < bugsAmount; i++) 
        {
            Bug.SpawnBug(_bugPrefab, transform);
        }
    }

    private void OnEnable()
    {
        TotalSworms += 1;
    }

    private void OnDisable()
    {
        TotalSworms -= 1;
    }

    private void Update()
    {
        Vector3 targetPosition = _target.GetCenteredPosition() + _targetOffset;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);

        if (AtTarget())
        {
            UpdateTargetOffset();
        }

        if (!_target.IsTargettingObject)
        {
            _target = TargetUtility.GetRandomTarget(transform.position, _randomTargetRadius, _ignoredLayers);
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<bullet>() != null)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        dealDamage damageable = other.GetComponent<dealDamage>();
        if (damageable != null)
        {
            damageable.dealDamage(_damagePerSecond * Time.deltaTime, Color.red, gameObject);
        }
    }

    public static BugSworm SpawnSworm(BugSworm swormPrefab, Vector3 spawnPosition, AttackTarget target)
    {
        BugSworm sworm = Instantiate(swormPrefab, spawnPosition, Quaternion.identity);
        sworm._target = target;
        sworm.UpdateTargetOffset();
        return sworm;
    }

    private void UpdateTargetOffset()
    {
        _targetOffset = Random.insideUnitSphere * _targetOffsetRadius;
        _targetOffset.y = 0;
    }

    private bool AtTarget(float maxDistance = 0.2f)
    {
        Vector3 targetPosition = _target.GetCenteredPosition() + _targetOffset;
        return Vector3.Distance(transform.position, targetPosition) <= maxDistance;
    }
}
