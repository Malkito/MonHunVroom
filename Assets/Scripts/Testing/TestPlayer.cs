using LordBreakerX.Health;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    private float _damageToEnemy = 10;

    [SerializeField]
    private Damageable _enemy;

    [ContextMenu("Damage Monster")]
    public void DoDamage()
    {
        _enemy.Damage(_damageToEnemy, gameObject);
    }
}
