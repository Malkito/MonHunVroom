using LordBreakerX.Utilities;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public class AttackController : NetworkBehaviour
    {
        #region Variables

        [SerializeField]
        private ScriptableAttackTable _attackTable;

        [SerializeField]
        private float _randomAttackRadius = 30;

        private Attack _activeAttack;

        private AttackTarget _target = new AttackTarget();

        #endregion

        #region Properties

        public AttackTarget Target { get { return _target; } }

        public bool IsAttacking { get { return _activeAttack != null; } }

        public Attack ActiveAttack { get => _activeAttack; }

        public bool RequestingAttack { get; private set; }

        #endregion

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsClient)
            {
                RequestActiveAttackServerRpc();
            }
        }

        #region Attack Updating Logic

        private void Update()
        {
            if (_activeAttack == null || !IsServer) return;

            if (_activeAttack.HasAttackFinished()) RequestStopAttack();
            else _activeAttack.OnAttackUpdate();
        }

        private void FixedUpdate()
        {
            if (_activeAttack == null || !IsServer) return;

            if (!_activeAttack.HasAttackFinished()) _activeAttack.OnAttackFixedUpdate();
        }

        #endregion

        #region Starting Attack

        public void RequestStartAttack()
        {
            if (IsServer)
            {
                StartAttack();
                StartAttackClientRpc();
            }
        }

        private void StartAttack()
        {
            if (IsAttacking || _attackTable == null) return;

            _activeAttack = _attackTable.GetRandomAttack(this);
            _activeAttack.OnStart();
        }

        [ClientRpc(RequireOwnership = false)]
        private void StartAttackClientRpc()
        {
            StartAttack();
        }

        public void RequestRandomAttack()
        {
            if (!IsServer) return;

            StartCoroutine(FindingAttackPosition());
        }

        private IEnumerator FindingAttackPosition()
        {
            RequestingAttack = true;
            Vector2 random = Random.insideUnitCircle * _randomAttackRadius;
            Vector3 attackPosition = new Vector3(random.x, transform.position.y, random.y);

            while (!NavMeshUtility.IsPathValid(transform.position, attackPosition))
            {
                random = Random.insideUnitCircle * _randomAttackRadius;
                attackPosition = new Vector3(random.x, transform.position.y, random.y);
                yield return null;
            }

            _target.Set(attackPosition);
            RequestStartAttack();
            RequestingAttack = false;
        }

        #endregion

        #region Stop Attack

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

        #endregion

        [ServerRpc(RequireOwnership = false)]
        private void RequestActiveAttackServerRpc()
        {
            if (_activeAttack != null)
            {
                StartAttackClientRpc();
            }
        }

    }
}


