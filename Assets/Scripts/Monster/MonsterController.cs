using LordBreakerX.Health;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    [SerializeField]
    private Transform[] _eyes;

    [SerializeField]
    private Vector3 _monsterBottom;

    [SerializeField]
    private ParticleSystem _stompEffect;

    [SerializeField]
    [Min(0)]
    private float _stompRadius = 1;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private NavMeshAgent _agent;

    private Dictionary<GameObject, float> _damageTable = new Dictionary<GameObject, float>();

    public Vector3 MonsterBottom { get { return _monsterBottom; } }

    public GameObject Target { get; private set; }

    private void Update()
    {
        // temp stuff
        if (_agent.velocity.sqrMagnitude >= 0.1f)
        {
            _animator.SetBool("walk", true);
        }
        else
        {
            _animator.SetBool("walk", false);
        }
    }

    public void OnDead()
    {
        _animator.SetBool("dead", true);
    }

    public Transform GetRandomEye()
    {
        if (_eyes.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, _eyes.Length);
        return _eyes[index];
    }

    public void OnMonsterHealthChanged(HealthInfo healthInfo)
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

        RaycastHit[] hits = Physics.SphereCastAll(transform.position + MonsterBottom, _stompRadius, Vector3.down, _stompRadius);
        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.CompareTag("Monster"))
            {
                dealDamage damage = hit.collider.gameObject.GetComponent<dealDamage>();
                if (damage != null) damage.dealDamage(50, Color.red, gameObject);
            }
            
        }
    }

    public void TailSwipe()
    {
        _animator.Play("tail swipe");
    }

    public bool TailSwipeFinished()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        return !stateInfo.IsName("tail swipe");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + _monsterBottom, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + _monsterBottom, _stompRadius);
        Gizmos.DrawWireSphere(transform.position + _monsterBottom, _stompRadius * 2);
    }
}
