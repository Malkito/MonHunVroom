using UnityEngine;

[RequireComponent(typeof(dealDamage))]
public class Throwable : MonoBehaviour
{
    dealDamage _health;

    private void Awake()
    {
        _health = GetComponent<dealDamage>();
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
}
