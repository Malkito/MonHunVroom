using LordBreakerX.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackholeController : MonoBehaviour
{
    [SerializeField]
    private LayerMask _ignoredLayers;

    [SerializeField]
    [Min(0f)]
    private float _pullStrength = 2000;

    [SerializeField]
    [Min(0f)]
    private float _pushStrength = 1000;

    [SerializeField]
    [Min(1)]
    private float _lifeSpan = 10f;

    [SerializeField]
    private float _moveSpeed = 5;

    [SerializeField]
    [Min(0f)]
    private float _damagePerSecond = 2;

    [SerializeField]
    private float _playerPullSpeed = 10;

    [SerializeField]
    private LayerMask _playerLayer;

    private Vector3 _moveDirection;

    private List<Rigidbody> _bodies = new List<Rigidbody>();

    private void Start()
    {
        StartCoroutine(DealDamageOverTime());
    }

    private void Update()
    {
        transform.position += _moveDirection * _moveSpeed * Time.deltaTime;
        _lifeSpan -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (_lifeSpan <= 0)
        {
            if (_bodies.Count > 0)
            {
                foreach (var body in _bodies)
                {
                    if (body == null) continue;

                    body.linearVelocity = Vector3.zero;
                    body.AddForce(Random.insideUnitSphere * _pushStrength, ForceMode.Force);
                }
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null || _ignoredLayers.Contains(other.gameObject.layer)) return;

        if (_playerLayer.Contains(other.gameObject.layer))
        {
            Rigidbody rigidbody = other.attachedRigidbody;
            rigidbody.transform.position = Vector3.MoveTowards(rigidbody.transform.position, transform.position, 10 * Time.deltaTime);
        }
        else
        {
            Rigidbody rigidbody = other.attachedRigidbody;
            Vector3 direction = (transform.position - rigidbody.transform.position).normalized;
            rigidbody.AddForce(direction * _pullStrength, ForceMode.Acceleration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_ignoredLayers.Contains(other.gameObject.layer)) return;
        if (other.attachedRigidbody != null) _bodies.Add(other.attachedRigidbody);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_ignoredLayers.Contains(other.gameObject.layer)) return;
        if (other.attachedRigidbody != null && _bodies.Contains(other.attachedRigidbody)) _bodies.Remove(other.attachedRigidbody);
    }

    public BlackholeController Clone(Vector3 position, Vector3 moveDirection)
    {
        BlackholeController clonedBlackhole = Instantiate(this, position, Quaternion.identity);
        clonedBlackhole._moveDirection = moveDirection;
        return clonedBlackhole;
    }

    public BlackholeController Clone(Vector3 position)
    {
        Vector2 random = Random.insideUnitCircle;
        return Clone(position, new Vector3(random.x, 0, random.y));
    }

    private IEnumerator DealDamageOverTime()
    {
        WaitForSeconds second = new WaitForSeconds(1);
        Dictionary<Rigidbody, dealDamage> damageRegistry = new Dictionary<Rigidbody, dealDamage>();

        while (true)
        {
            if (_bodies.Count > 0)
            {
                foreach (Rigidbody rigidbody in _bodies)
                {
                    if (rigidbody == null) continue;

                    if (!damageRegistry.ContainsKey(rigidbody))
                    {
                        damageRegistry.Add(rigidbody, rigidbody.GetComponent<dealDamage>());
                    }

                    damageRegistry[rigidbody]?.dealDamage(_damagePerSecond, Color.red, gameObject);
                }
            }

            yield return second;
        }
    }
}
