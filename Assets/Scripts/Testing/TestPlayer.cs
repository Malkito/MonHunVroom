using LordBreakerX.Health;
using Unity.Netcode;
using UnityEngine;

public class TestPlayer : NetworkBehaviour
{
    [SerializeField]
    [Min(0)]
    private float _damageToEnemy = 10;

    [SerializeField]
    private MonsterHealth _enemy;

    private GameObject _attackPlayer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerShooting[] players = FindObjectsByType<playerShooting>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach(var player in players)
        {
            if (player.IsOwner)
            {
                _attackPlayer = player.gameObject;
            }
        }
    }

    [ContextMenu("Damage Monster")]
    public void DoDamage()
    {
        if (_attackPlayer != null) 
        {
            Debug.Log("Attacking");
            _enemy.dealDamage(_damageToEnemy, Color.red, _attackPlayer.gameObject);
        }
        else
        {
            _enemy.dealDamage(_damageToEnemy, Color.red, null);
        }
    }
}
