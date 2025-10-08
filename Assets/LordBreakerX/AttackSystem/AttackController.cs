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

        private List<TargetPlayer> _targetPlayers = new List<TargetPlayer>();

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
            _targetPlayers.Add(new TargetPlayer(playerObject, clientID));
        }

        private void OnClientDisconnected(ulong clientID)
        {
            TargetPlayer player = TargetPlayer.GetPlayerWithID(clientID, _targetPlayers);

            if (player != null) 
            {
                _targetPlayers.Remove(player);
            }
        }

        private void Awake()
        {
            _internalAttackTable = _attackTable.CreateTable(this);
        }

        protected virtual void Update()
        {
            if (_activeAttack == null || !IsServer) return;

            if (_activeAttack.HasAttackFinished()) StopAttack();
            else _activeAttack.OnAttackUpdate();
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
            if (_targetPlayers.Count == 0) return;

            int randomPlayerIndex = Random.Range(0, _targetPlayers.Count);
            _target.Set(_targetPlayers[randomPlayerIndex].PlayerTransform, transform.position);

            Debug.Log($"{_targetPlayers[randomPlayerIndex]} -- {_targetPlayers[randomPlayerIndex]}");

            StartRandomAttack();
        }

    }
}


