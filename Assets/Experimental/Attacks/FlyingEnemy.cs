using LordBreakerX.Utilities.AI;
using LordBreakerX.Utilities.Math;
using UnityEngine;
using UnityEngine.AI;

public class FlyingEnemy : MonoBehaviour
{
    [SerializeField]
    private float _wanderRadius = 10;

    [SerializeField]
    private NavMeshAgent _agent;

    [SerializeField]
    private float _minFlyingHeight = 5;

    [SerializeField]
    private float _maxFlyingHeight = 10;

    [SerializeField]
    private float _changeHeightRate = 3;

    [SerializeField]
    private Transform _model;

    [SerializeField]
    private float _verticalFlySpeed = 5;

    [SerializeField]
    private float _reachDistance = 0.2f;

    private float _flyHeight;

    private void Awake()
    {
        ChangeFlight();
        InvokeRepeating("ChangeFlight", _changeHeightRate, _changeHeightRate);
    }

    private void Update()
    {
        Vector3 flyPosition = transform.position + new Vector3(0, _flyHeight, 0);
        _model.transform.position = Vector3.MoveTowards(_model.transform.position, flyPosition, _verticalFlySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _agent.destination) <= _reachDistance)
        {
            SetRandomDestination();
        }
    }

    public void ChangeFlight()
    {
        float random = Random.Range(0, 100);
        _flyHeight = PercentageUtility.MapPercentage(random, _minFlyingHeight, _maxFlyingHeight);
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
}
