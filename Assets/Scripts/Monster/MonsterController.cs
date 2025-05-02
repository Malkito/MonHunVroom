using LordBreakerX.AbilitySystem;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : NetworkBehaviour
{
    [SerializeField]
    private NavMeshAgent _agent;

    [SerializeField]
    private AbilityHandler _abilityHandler;

    private Animator _animator;

    private NetworkVariable<Vector3> _targetPosition = new NetworkVariable<Vector3>();

    public Vector3 TargetPosition { get {  return _targetPosition.Value; } }

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

    #endregion

    public void RequestStartRandomAttack()
    {
        if (IsServer)
        {
            //_abilityHandler.StartRandomAbility();
            //StartRandomAttackClientRpc();
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void StartRandomAttackClientRpc()
    {
        // update this method once attacking has been fully made network ready
    } 
}
