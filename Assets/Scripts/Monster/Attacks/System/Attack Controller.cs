using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AttackController : NetworkBehaviour
{
    [SerializeField]
    private List<AttackTable> _attackTables = new List<AttackTable>();

    [SerializeField]
    private AttackTable _fallbackTable;

    private Attack _activeAttack;

    private TargetResolver _provider = new TargetResolver();

    public bool IsAttacking { get { return _activeAttack != null; } }

    public bool IsRequestingAttack { get; private set; }

    public TargetResolver TargetProvider { get { return _provider; } }

    public Vector3 TargetPosition { get => _provider.GetTargetPosiiton(); }

    public bool HasTarget { get => _provider.HasTarget; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        List<AttackTable> copiedTables = new List<AttackTable>();

        foreach (AttackTable attackTable in _attackTables)
        {
            copiedTables.Add(AttackTable.Copy(attackTable, this));
        }

        _attackTables = copiedTables;

        if (IsClient)
        {
            RequestActiveAttackServerRpc();
        }
    }

    private void Update()
    {
        if (_activeAttack != null)
        {
            _activeAttack.OnUpdate();
            if (_activeAttack.CanFinishAttack()) RequestStopAttack();
        }
    }

    private void OnDrawGizmos()
    {
        if (_attackTables.Count > 0)
        {
            foreach(AttackTable attackTable in _attackTables)
            {
                if (attackTable != null) attackTable.DrawGizmos(this);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_attackTables.Count > 0)
        {
            foreach(AttackTable attackTable in _attackTables)
            {
                if (attackTable != null) attackTable.DrawGizmosSelected(this);
            }
        }
    }

    public void RequestStartAttack()
    {
        IsRequestingAttack = true;

        if (IsServer)
        {
            StartAttack();
            StartAttackClientRpc();
            IsRequestingAttack = false;
        }
    }

    private void StartAttack()
    {
        AttackTable attackTable = GetTable();

        if (IsAttacking || attackTable == null) return;

        _activeAttack = attackTable.GetRandomAttack();
        _activeAttack.OnStart();
    }

    private AttackTable GetTable()
    {
        List<AttackTable> useableTables = new List<AttackTable>();

        foreach (AttackTable attackTable in _attackTables)
        {
            if (attackTable.CanUse(this)) useableTables.Add(attackTable);
        }

        if (useableTables.Count > 0)
        {
            int usedTableIndex = Random.Range(0, useableTables.Count);

            Debug.Log($"Gotten Attack Table: {useableTables[usedTableIndex]} [{usedTableIndex + 1} / {useableTables.Count}]");
            return useableTables[usedTableIndex];
        }

        return _fallbackTable;
    }

    [ClientRpc(RequireOwnership = false)]
    private void StartAttackClientRpc()
    {
        StartAttack();
        IsRequestingAttack = false;
    }

    public void RequestStopAttack()
    {
        if (IsServer)
        {
            StopAttack();
            StopAttackClientRpc();
        }
    }

    private void StopAttack()
    {
        if (_activeAttack != null) _activeAttack.OnStop();
        _activeAttack = null;
    }

    [ClientRpc(RequireOwnership = false)]
    private void StopAttackClientRpc()
    {
        StopAttack();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestActiveAttackServerRpc()
    {
        if (_activeAttack != null)
        {
            StartAttackClientRpc();
        }
    }

}
