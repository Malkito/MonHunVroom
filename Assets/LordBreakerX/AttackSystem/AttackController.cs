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
        #region Variables

        [SerializeField]
        [Min(1)]
        private float _respawnAttackCooldown = 15;

        [SerializeField]
        private float _randomAttackRadius;

        [SerializeField]
        private ScriptableAttackTable _attackTable;

        [SerializeField]
        private LayerMask _ignoreLayers;

        private ScriptableAttack _activeAttack;

        private List<Transform> _attackablePlayers = new List<Transform>();

        private List<PlayerAttackCooldown> _playersInCooldown = new List<PlayerAttackCooldown>();

        private AttackTable _internalTable;

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
            _internalTable = _attackTable.CreateTable(this);

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

                _attackablePlayers.Add(client.PlayerObject.transform);
            }
        }
        #endregion

        #region Unity Callbacks

        private void Update()
        {
            if (!IsServer) return;

            if (_activeAttack != null)
            {
                if (_activeAttack.HasAttackFinished())
                    StopAttack();
                else
                    _activeAttack.OnAttackUpdate();
            }

            if (_playersInCooldown.Count > 0)
            {
                _playersInCooldown.RemoveAll(CooldownFinishedPredicate);
            }
        }

        private void FixedUpdate()
        {
            if (_activeAttack == null || !IsServer) return;

            if (!_activeAttack.HasAttackFinished()) 
                _activeAttack.OnAttackFixedUpdate();
        }

        private void OnEnable()
        {
            playerRespawn.OnPlayerRespawn += OnPlayerRespawn;
        }

        private void OnDisable()
        {
            playerRespawn.OnPlayerRespawn -= OnPlayerRespawn;
        }

        #endregion

        #region Starting Attacks
        private void StartAttack(ScriptableAttack attack)
        {
            if (attack == null || IsAttacking || !IsServer) return;

            _activeAttack = attack;
            _activeAttack.OnAttackStarted();
        }

        public void StartRandomAttack()
        {
            ScriptableAttack randomAttack = _internalTable.GetRandomEntry();
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
            if (TryTargetPlayer())
            {
                StartRandomAttack();
            }
        }

        public bool TryTargetPlayer()
        {
            if (_attackablePlayers.Count > 0)
            {
                int randomPlayerIndex = Random.Range(0, _attackablePlayers.Count);
                Transform player = _attackablePlayers[randomPlayerIndex];

                Target = new AttackTarget(player, transform.position);
                return true;
            }

            return false;
        }

        public void AttackRandomObject<THealth>()
        {
            if (TryTargetRandomObject<THealth>())
            {
                StartRandomAttack();
            }
        }

        public bool TryTargetRandomObject<THealth>()
        {
            Target = TargetUtility.GetRandomTarget<THealth>(this);

            return Target.IsTargettingObject && Target.GetPosition() != transform.position;
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


        #region Respawn Attack Inturuptions

        public bool HasAttackablePlayer()
        {
            return _attackablePlayers != null && _attackablePlayers.Count > 0;
        }

        private void OnPlayerRespawn(NetworkObject respawnedPlayer)
        {
            if (!IsServer) return;

            Transform playerTransform = respawnedPlayer.transform;
            PlayerAttackCooldown cooldown = (PlayerAttackCooldown)playerTransform;

            if (_attackablePlayers.Contains(playerTransform))
                _attackablePlayers.Remove(playerTransform);

            if (_playersInCooldown.Contains(cooldown))
                _playersInCooldown.Remove(cooldown);

            _playersInCooldown.Add(new PlayerAttackCooldown(playerTransform, _respawnAttackCooldown));

            if (IsAttacking && Target.TargetTransform == playerTransform)
            {
                StopAttack();
            }
        }

        private bool CooldownFinishedPredicate(PlayerAttackCooldown cooldown)
        {
            if (cooldown.UpdateCooldown())
            {
                if (!_attackablePlayers.Contains(cooldown.PlayerTransform))
                {
                    _attackablePlayers.Add(cooldown.PlayerTransform);
                }
                return true;
            }

            return false;
        }

        #endregion
    }
}


