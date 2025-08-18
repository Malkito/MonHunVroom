using LordBreakerX.Utilities.AI;
using UnityEngine;
using UnityEngine.AI;

public class UndergroundBarrageController : MonoBehaviour
{
    [SerializeField]
    private float _wanderRadius = 10;

    [SerializeField]
    private NavMeshAgent _agent;

    [SerializeField]
    private float _reachDistance = 0.2f;

    [SerializeField]
    private float _minThrowSrength = 100;

    [SerializeField]
    private float _maxThrowSrength = 200;

    [SerializeField]
    private float _minThrowRate = 2;

    [SerializeField]
    private float _maxThrowRate = 5;

    [SerializeField]
    private float _throwChance = 65;

    [SerializeField]
    private int _maxThrowAmount = 3;

    [SerializeField]
    private Roubble _roubblePrefab;

    private float _throwDelay;

    private void Awake()
    {
        SetRandomDestination();
        ResetThrowDelay();
    }

    private void ResetThrowDelay()
    {
        _throwDelay = Random.Range(_minThrowRate, _maxThrowRate);
    }

    private void Update()
    {
        _throwDelay -= Time.deltaTime;

        if (_throwDelay <= 0)
        {
            ResetThrowDelay();

            for (int x = 0; x < _maxThrowAmount; x++) 
            {
                float chance = Random.Range(1.0f, 100);
                if (chance <= _throwChance)
                {
                    _roubblePrefab.CreateRouble(transform.position, _minThrowSrength, _maxThrowSrength);
                }
            }
        }

        if (Vector3.Distance(transform.position, _agent.destination) <= _reachDistance)
        {
            SetRandomDestination();
        }
    }

    private void SetRandomDestination()
    {
        Vector3 randomPosition = NavMeshUtility.GetRandomPosition(transform.position, _wanderRadius);
        var path = new NavMeshPath();

        if (RandomPathGenerator.IsPathValid(path, transform.position, randomPosition))
        {
            _agent.SetDestination(randomPosition);
        }
        else
        {
            _agent.SetDestination(transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _wanderRadius);
    }

}
