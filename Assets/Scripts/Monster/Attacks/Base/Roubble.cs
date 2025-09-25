using LordBreakerX.Attributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Roubble : MonoBehaviour
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

    private void Awake()
    {
        Invoke("Death", _lifeSpan);
    }

    public void Death()
    {
        Destroy(gameObject);
    }

    public Roubble CreateRouble(Vector3 startPosition, ThrowStrength throwStrength)
    {
        Roubble roubbleCopy = Instantiate(this, startPosition, Quaternion.identity);

        Vector3 throwForce = throwStrength.GetForce();

        roubbleCopy._rigidbody.AddForce(throwForce, ForceMode.Force);

        return roubbleCopy;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_monsterTag)) return;

        dealDamage damageable = other.GetComponent<dealDamage>();
        if (damageable != null) damageable.dealDamage(_damageOnImpact, Color.red, gameObject);
    }
}
