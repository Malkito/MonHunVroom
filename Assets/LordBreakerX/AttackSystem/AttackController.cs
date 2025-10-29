using LordBreakerX.Tables;
using LordBreakerX.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public class AttackController : NetworkBehaviour
    {
        [SerializeField]
        private ScriptableAttackTable _attackTable;

        [SerializeField]
        private float _randomAttackRadius = 30;

        private Attack _activeAttack;

        private AttackTarget _target = new AttackTarget();

        private WeightTable<Attack> _internalAttackTable;

        private List<AttackablePlayer> _attackablePlayers = new List<AttackablePlayer>();

        public Vector3 TargetPosition { get => _target.GetTargetPosition(); }

        public Vector3 CenteredTargetPosition { get => _target.GetCenteredTargetPosition(); }

        public bool IsAttacking { get { return _activeAttack != null; } }


        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton != null && IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientID)
        {
            StartCoroutine(WaitForPlayerObject(clientID));
        }

        private IEnumerator WaitForPlayerObject(ulong clientID)
        {
            while (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientID) ||
                    NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject == null) 
            {
                yield return null;
            }

            NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
            AttackablePlayer player = new AttackablePlayer(playerObject, clientID);
            _attackablePlayers.Add(player);
        }

        private void OnClientDisconnected(ulong clientID)
        {
            AttackablePlayer attackablePlayer = AttackablePlayer.GetPlayerEntryWithID(clientID, _attackablePlayers);

            if (attackablePlayer != null) 
            {
                _attackablePlayers.Remove(attackablePlayer);
            }
        }

        private void Awake()
        {
            _internalAttackTable = _attackTable.CreateTable(this);
        }

        protected virtual void Update()
        {
            if (_activeAttack == null || !IsServer) return;

            _activeAttack.OnAttackUpdate();
            if (_activeAttack != null && _activeAttack.HasAttackFinished()) StopAttack();
        }

        private void FixedUpdate()
        {
            if (_activeAttack == null || !IsServer) return;

            if (!_activeAttack.HasAttackFinished()) _activeAttack.OnAttackFixedUpdate();
        }

        private void StartAttack(Attack attack)
        {
            if (attack == null || IsAttacking || !IsServer) return;

            _activeAttack = attack;
            _activeAttack.OnStart();
        }

        private void StartRandomAttack()
        {
            Attack randomAttack = _internalAttackTable.GetRandomEntry();
            StartAttack(randomAttack);
        }

        public void StopAttack()
        {
            if (_activeAttack == null || !IsServer) return;

            _activeAttack.OnStop();
            _activeAttack = null;
        }

        public void AttackRandomPosition()
        {
            Vector3 attackPosition = Random.insideUnitCircle * _randomAttackRadius;
            attackPosition = new Vector3(attackPosition.x, transform.position.y, attackPosition.y);

            if (NavMeshUtility.IsPathValid(transform.position, attackPosition))
            {
                _target.Set(attackPosition);
                StartRandomAttack();
            }
        }

        public virtual void AttackRandomPlayer()
        {
            if (_attackablePlayers.Count <= 0) return;

            int randomPlayerIndex = Random.Range(0, _attackablePlayers.Count);
            AttackablePlayer player = _attackablePlayers[randomPlayerIndex];

            _target.Set(player.PlayerTransform, transform.position);

            StartRandomAttack();
        }

    }
}


