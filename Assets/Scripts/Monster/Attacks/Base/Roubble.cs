using LordBreakerX.Attributes;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(NetworkObject))]
public class Roubble : NetworkBehaviour
{
    [SerializeField]
    private float _lifeSpan = 10;

    [SerializeField]
    private Rigidbody _rigidbody;

    [SerializeField]
    private float _damageOnImpact;

    [SerializeField]
    [TagDropdown]
    private string _monsterTag = "Monster";

    private Vector3 _launchForce;
    private bool _launchPending;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Invoke(nameof(Death), _lifeSpan);
    }

    public override void OnNetworkDespawn()
    {
        CancelInvoke(nameof(Death));
        _launchPending = false;
    }

    private void FixedUpdate()
    {
        if (!IsSpawned || !IsServer || !_launchPending)
            return;

        // NetworkRigidbody keeps the body kinematic until spawning has finished.
        _rigidbody.AddForce(_launchForce, ForceMode.Force);
        _launchPending = false;
    }

    public void Death()
    {
        if (IsSpawned && IsServer)
            NetworkObject.Despawn();
    }

    public Roubble CreateRouble(float damage, Vector3 startPosition, ThrowStrength throwStrength)
    {
        Roubble roubbleCopy = Instantiate(this, startPosition, Quaternion.identity);
        roubbleCopy._launchForce = throwStrength.GetForce();
        roubbleCopy._launchPending = true;
        roubbleCopy._damageOnImpact = damage;
        return roubbleCopy;
    }

    public Roubble CreateRouble(float damage, Vector3 startPosition, Vector3 finalPosition)
    {
        Vector3 direction = finalPosition - startPosition;
        float distance = Vector3.Distance(startPosition, finalPosition);

        Roubble roubleInstance = Instantiate(this, startPosition, Quaternion.identity);
        roubleInstance._launchForce = distance * roubleInstance._rigidbody.mass * direction;
        roubleInstance._launchPending = true;
        roubleInstance._damageOnImpact = damage;
        return roubleInstance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsSpawned || !IsServer || other.CompareTag(_monsterTag))
            return;

        dealDamage damageable = other.GetComponent<dealDamage>();
        if (damageable != null)
            damageable.dealDamage(_damageOnImpact, Color.red, gameObject);
    }
}
