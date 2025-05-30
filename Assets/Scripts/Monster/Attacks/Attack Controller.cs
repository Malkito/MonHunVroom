using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AttackController : NetworkBehaviour
{
    [SerializeField]
    private List<Attack> _attacksToRegister = new List<Attack>();

    private Dictionary<string, Attack> _registeredAttacks = new Dictionary<string, Attack>();

    private List<Attack> _attacks = new List<Attack>(); // list version of registered attacks

    private Attack _activeAttack;

    public bool IsAttacking { get { return _activeAttack != null; } }

    public bool IsRequestingAttack { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        foreach (Attack attack in _attacksToRegister)
        {
            RegisterAttack(attack);
        }

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

    public void RegisterAttack(Attack attack)
    {
        if (attack == null)
        {
            Debug.LogWarning($"Could not register attack since no attack asset was provided!");
        }
        else if (_registeredAttacks.ContainsKey(attack.ID))
        {
            Debug.LogWarning($"Could not register attack since the ID {attack.ID} is already taken!");
        }
        else
        {
            Attack copiedAttack = Instantiate(attack);
            copiedAttack.Initilize(gameObject);
            _registeredAttacks.Add(attack.ID, copiedAttack);
            _attacks.Add(attack);
        }
    }

    public void RequestStartAttack(Attack attack)
    {
        IsRequestingAttack = true;

        if (IsServer)
        {
            StartAttack(attack.ID);
            StartAttackClientRpc(attack.ID);
            IsRequestingAttack = false;
        }
    }

    private void StartAttack(string attackID)
    {
        if (_registeredAttacks.ContainsKey(attackID))
        {
            if (_activeAttack != null) _activeAttack.OnStop();
            _activeAttack = _registeredAttacks[attackID];
            if (_activeAttack != null) _activeAttack.OnStart();
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void StartAttackClientRpc(string attackID)
    {
        StartAttack(attackID);
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
            StartAttackClientRpc(_activeAttack.ID);
        }
    }

    public Attack StartRandomAttack()
    {
        if (_attacks.Count > 0)
        {
            int index = Random.Range(0, _attacks.Count);
            Attack attack = _attacks[index];
            RequestStartAttack(attack);
            return attack;
        }
        else
        {
            return null;
        }
    }
}
