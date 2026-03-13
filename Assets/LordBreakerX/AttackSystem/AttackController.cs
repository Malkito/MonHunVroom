using LordBreakerX.Tables;
using LordBreakerX.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public class AttackController : NetworkBehaviour
    {
        #region Variables

        [SerializeField]
        private float _randomAttackRadius;

        [SerializeField]
        private ScriptableAttackTable _attackTable;

        [SerializeField]
        private LayerMask _ignoreLayers;

        private ScriptableAttack _activeAttack;

        private List<AttackablePlayer> _attackablePlayers = new List<AttackablePlayer>();

        #endregion

        #region Properties

        public float RandomAttackRadius => _randomAttackRadius;

        public bool IsAttacking => _activeAttack != null;

        public LayerMask IgnoredLayers => _ignoreLayers;

        public AttackTarget Target { get; private set; }

        #endregion

        #region Adding Players

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                StartCoroutine(WaitForPlayers());
            }
        }

        private IEnumerator WaitForPlayers()
        {
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                while(client.PlayerObject == null)
                {
                    yield return null;
                }

                AttackablePlayer player = new AttackablePlayer(client.PlayerObject, client.ClientId);
                _attackablePlayers.Add(player);
            }

            Debug.Log(_attackablePlayers.Count);
        }

        //public override void OnNetworkSpawn()
        //{
        //    if (IsServer)
        //    {
        //        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        //        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        //    }
        //}

        //public override void OnNetworkDespawn()
        //{
        //    if (NetworkManager.Singleton != null && IsServer)
        //    {
        //        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        //        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        //    }
        //}

        //private void OnClientConnected(ulong clientID)
        //{
        //    Debug.Log("ClientConnected");
        //    StartCoroutine(WaitForPlayerObject(clientID));
        //}

        //private IEnumerator WaitForPlayerObject(ulong clientID)
        //{
        //    while (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientID) ||
        //            NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject == null) 
        //    {
        //        yield return null;
        //    }

        //    NetworkObject playerObject = NetworkManager.Singleton.ConnectedClients[clientID].PlayerObject;
        //    AttackablePlayer player = new AttackablePlayer(playerObject, clientID);
        //    _attackablePlayers.Add(player);
        //}

        //private void OnClientDisconnected(ulong clientID)
        //{
        //    AttackablePlayer attackablePlayer = AttackablePlayer.GetPlayerEntryWithID(clientID, _attackablePlayers);

        //    if (attackablePlayer != null) 
        //    {
        //        _attackablePlayers.Remove(attackablePlayer);
        //    }
        //}

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (_activeAttack == null || !IsServer) return;

            if (_activeAttack.HasAttackFinished()) 
                StopAttack();
            else
                _activeAttack.OnAttackUpdate();

        }

        private void FixedUpdate()
        {
            if (_activeAttack == null || !IsServer) return;

            if (!_activeAttack.HasAttackFinished()) 
                _activeAttack.OnAttackFixedUpdate();
        }

        #endregion

        #region Starting Attacks
        private void StartAttack(ScriptableAttack attack)
        {
            if (attack == null || IsAttacking || !IsServer) return;

            _activeAttack = attack;
            _activeAttack.OnAttackStarted();
        }

        private void StartRandomAttack()
        {
            ScriptableAttack randomAttack = _attackTable.GetRandomEntry(this);
            StartAttack(randomAttack);
        }

        public void AttackRandomPosition()
        {
            Vector3 attackPosition = Random.insideUnitCircle * _randomAttackRadius;
            attackPosition = new Vector3(attackPosition.x, transform.position.y, attackPosition.y);

            if (NavMeshUtility.IsPathValid(transform.position, attackPosition))
            {
                Target = new AttackTarget(attackPosition);
                StartRandomAttack();
            }
        }

        public void AttackRandomPlayer()
        {
            if (_attackablePlayers.Count <= 0) return;

            int randomPlayerIndex = Random.Range(0, _attackablePlayers.Count);
            AttackablePlayer player = _attackablePlayers[randomPlayerIndex];

            Target = new AttackTarget(player.PlayerTransform, transform.position);

            StartRandomAttack();
        }

        public void AttackRandomObject<THealth>()
        {
            Target = TargetUtility.GetRandomTarget<THealth>(this);

            if (Target.GetPosition() != transform.position)
            {
                StartRandomAttack();
            }
        }
        #endregion

        #region Stopping Attacks
        public void StopAttack()
        {
            if (_activeAttack == null || !IsServer) return;

            _activeAttack.OnAttackStopped();
            _activeAttack = null;
        }

        #endregion

        #region Spawning Projectiles
        public GameObject SpawnProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!IsServer) return null;

            GameObject projectile = Instantiate(prefab, position, rotation);

            SpawnProjectile(projectile);

            return projectile;
        }

        public void SpawnProjectile(GameObject prefabInstance)
        {
            if (!IsServer) return;

            NetworkObject networkInstance = prefabInstance.GetComponent<NetworkObject>();

            if (networkInstance != null)
                networkInstance.Spawn();

#if UNITY_EDITOR
            else
                Debug.LogWarning("Projectile does not contain a network object so it can not be synced!");
#endif
        }
        #endregion

    }
}


