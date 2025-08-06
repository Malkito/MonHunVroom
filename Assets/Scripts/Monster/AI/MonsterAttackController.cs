using LordBreakerX.Attributes;
using LordBreakerX.Health;
using LordBreakerX.States;
using Unity.Netcode;
using UnityEngine;

public class MonsterAttackController : AttackController
{
    [SerializeField]
    [RequiredField]
    [Header("Laser Eyes")]
    private Transform[] _eyes;

    [SerializeField]
    [RequiredField]
    private Laser _laserPrefab;

    [Header("Targetting Player Properties")]
    [SerializeField]
    [Min(0f)]
    private float _timeBetweenPlayerAttacks = 30;

    private DamageTable _recentDamageTable = new DamageTable();

    private Timer _playerAttackTimer;

    private StateMachineNetworked _machine;

    public Timer PlayerAttackTimer { get { return _playerAttackTimer; } }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _machine = GetComponent<StateMachineNetworked>();
        _playerAttackTimer = new Timer(_timeBetweenPlayerAttacks);
        _playerAttackTimer.OnTimerFinished += () => { _machine.ChangeStateWhen(MonsterStates.TARGET_PLAYER, () => !IsAttacking && !IsRequestingAttack); };
    }

    //------------------------------
    // Laser Eyes Methods
    //------------------------------

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
        if (!IsHost || !IsServer) ShootLaser(_laserPrefab, attackPosition);
    }

    //-------------------------------------
    // Targeting Player Methods
    //-------------------------------------

    public void OnMonsterHealthChanged(HealthInfo healthInfo)
    {
        if (IsServer)
            _recentDamageTable.UpdateTable(healthInfo.Source, healthInfo.DamageCaused);
    }

    public void ResetDamageTable()
    {
        if (IsServer) 
            _recentDamageTable.ResetTable();
    }

    public void UpdateTarget()
    {
        if (!IsServer) return;

        GameObject target = _recentDamageTable.GetMostDamageTarget();

        if (target == null)  TargetProvider.SetTargetPosition(transform.position);
        else TargetProvider.SetTarget(target.transform, transform.position);
    }
}

