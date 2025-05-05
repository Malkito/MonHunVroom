using LordBreakerX.States;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : NetworkBehaviour
{
    [SerializeField]
    private Transform[] _eyes;
    [SerializeField]
    private NavMeshAgent _agent;

    [SerializeField]
    private AttackController _attackController;

    [SerializeField]
    private StateMachineNetworked _stateMachine;

    [SerializeField]
    private Laser _laserPrefab;

    private Animator _animator;

    private NetworkVariable<Vector3> _targetPosition = new NetworkVariable<Vector3>();

    public Vector3 TargetPosition { get {  return _targetPosition.Value; } }

    public AttackController AttackHandler {  get { return _attackController; } }

    public StateMachineNetworked Machine { get { return _stateMachine; } }

    public bool DestinationReachable { get { return _agent.pathStatus == NavMeshPathStatus.PathComplete; } }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _animator = GetComponent<Animator>();

        _targetPosition.OnValueChanged += OnTargetPositionChanged;
    }

    #region Movement Logic

    private void OnTargetPositionChanged(Vector3 previousValue, Vector3 newValue)
    {
        _agent.SetDestination(newValue);
    }

    public void StopMovement()
    {
        if (IsServer)
        {
            _targetPosition.Value = _agent.transform.position;
        }
    }

    public void RandomDestination(float range)
    {
        if (IsServer)
        {
            _targetPosition.Value = NavMeshUtility.GetRandomPosition(_agent.transform.position, range);
        }
    }

    public void UpdateWalkAnimation()
    {
        if (_agent.velocity.sqrMagnitude >= 0.1f)
        {
            _animator.SetBool("walk", true);
        }
        else
        {
            _animator.SetBool("walk", false);
        }
    }

    public void RequestStartRandomAttack()
    {
        _attackController.StartRandomAttack();
    }
    #endregion

    #region Attacking Logic

    public void RequestShootLaser(Laser prefab, Vector3 attackPosition)
    {
        if (IsServer)
        {
            ShootLaser(prefab, attackPosition);
            ShootLaserClientRpc(attackPosition);
        }
    }

    private void ShootLaser(Laser prefab, Vector3 attackPosition)
    {
        int randomEyeIndex = Random.Range(0, _eyes.Length);
        Vector3 eyePosition = _eyes[randomEyeIndex].position;

        Laser.CreateLaser(prefab, gameObject, eyePosition, attackPosition);
    }

    [ClientRpc(RequireOwnership = false)]
    private void ShootLaserClientRpc(Vector3 attackPosition)
    {
        ShootLaser(_laserPrefab, attackPosition);
    }

    #endregion
}
