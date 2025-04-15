using LordBreakerX.Health;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    private Transform[] _eyes;

    [SerializeField]
    private Vector3 _monsterBottom;

    [SerializeField]
    private ParticleSystem _stompEffect;

    private Dictionary<GameObject, float> _damageTable = new Dictionary<GameObject, float>();

    public Vector3 MonsterBottom { get { return _monsterBottom; } }

    public GameObject Target { get; private set; }

    public Transform GetRandomEye()
    {
        if (_eyes.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, _eyes.Length);
        return _eyes[index];
    }

    public void OnMonsterDamaged(HealthInfo healthInfo)
    {
        Debug.Log($"Called Damaged Monster with following: (CH:{healthInfo.CurrentHealth}, MH:{healthInfo.Maxhealth}, HS:{healthInfo.Source != null}, DC:{healthInfo.DamageCaused})");

        if (healthInfo.Source == null || healthInfo.DamageCaused <= 0) return;

        if (_damageTable.ContainsKey(healthInfo.Source)) 
            _damageTable[healthInfo.Source] += healthInfo.DamageCaused;
        else
            _damageTable[healthInfo.Source] = healthInfo.DamageCaused;
    }

    public void ResetDamageTable()
    {
        _damageTable.Clear();
    }

    public void UpdateTarget()
    {
        if (_damageTable.Count > 0)
        {
            KeyValuePair<GameObject, float> highestPair = new KeyValuePair<GameObject, float>(null, 0);

            foreach(KeyValuePair<GameObject, float> damagePair in _damageTable)
            {
                if (damagePair.Value > 0 && damagePair.Value > highestPair.Value)
                {
                    highestPair = damagePair;
                }
            }

            Target = highestPair.Key;
        }
        else
        {
            Target = null;
        }
    }

    public void Stomp()
    {
        _stompEffect.Play();
    }

    public void TailSwipe()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + _monsterBottom, 0.1f);
    }
}
