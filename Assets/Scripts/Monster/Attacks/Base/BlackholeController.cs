using LordBreakerX.Utilities;
using System.Collections.Generic;
using Unity.Netcode;
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

    private List<Rigidbody> _impactedRigs = new List<Rigidbody>();

    private Vector3 _moveDirection;

    private void Awake()
    {
        Vector2 random = Random.insideUnitCircle;
        _moveDirection = new Vector3(random.x, 0, random.y);
    }

    private void Update()
    {
        transform.position += _moveDirection * _moveSpeed * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        _lifeSpan -= Time.fixedDeltaTime;

        if (_lifeSpan <= 0)
        {
            foreach (var body in _impactedRigs) 
            {
                body.linearVelocity = Vector3.zero;
                body.AddForce(Random.insideUnitSphere * _pushStrength * body.mass, ForceMode.Force);
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null && !_ignoredLayers.Contains(other.gameObject.layer)) 
            _impactedRigs.Add(other.attachedRigidbody);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null || _ignoredLayers.Contains(other.gameObject.layer)) return;
        
        Rigidbody rigidbody = other.attachedRigidbody;

        float gravityIntensity = Vector3.Distance(transform.position, other.transform.position) / 1;
        Vector3 direction = (transform.position - rigidbody.transform.position).normalized;
        float gForce = gravityIntensity * _pullStrength * rigidbody.mass * Time.smoothDeltaTime;
        rigidbody.AddForce(direction *  gForce, ForceMode.Acceleration);
    }

    private void OnTriggerExit(Collider other)
    {
        if (_impactedRigs.Contains(other.attachedRigidbody)) 
            _impactedRigs.Remove(other.attachedRigidbody);
    }
}
