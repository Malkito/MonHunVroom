using LordBreakerX.Utilities.AI;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace LordBreakerX.AttackSystem
{
    public class AttackController : NetworkBehaviour
    {
        #region Variables

        [SerializeField]
        private AttackTable _attackTable;

        [SerializeField]
        private float _targetOffset;

        [SerializeField]
        private float _randomAttackRadius = 30;

        private Attack _activeAttack;

        private TargetResolver _provider;

        #endregion

        #region Properties

        public bool IsAttacking { get { return _activeAttack != null; } }

        public bool HasTarget { get => _provider.HasTarget; }

        public Vector3 TargetPosition { get => _provider.GetPosiiton(); }
        public Vector3 OffsettedTargetPosition { get => _provider.GetOffsettedPosition(transform.position); }

        public Attack ActiveAttack { get => _activeAttack; }

        public TargetResolver TargetProvider { get { return _provider; } }

        public bool RequestingAttack { get; private set; }

        #endregion

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsClient)
            {
                RequestActiveAttackServerRpc();
            }

            _provider = new TargetResolver(_targetOffset);
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

            AttackCreator creator = _attackTable.GetRandomObject();
            _activeAttack = creator.Create(this);
            Debug.Log($"{creator} -- {creator.Create(this)}");
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

            while (!RandomPathGenerator.IsPathValid(new NavMeshPath(), transform.position, attackPosition))
            {
                random = Random.insideUnitCircle * _randomAttackRadius;
                attackPosition = new Vector3(random.x, transform.position.y, random.y);
                yield return null;
            }

            _provider.SetTarget(attackPosition);
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

        #region Drawing

        private void OnDrawGizmos()
        {
            if (_provider != null)
            {
                _provider.DrawTarget(transform.position);
            }
        }

        #endregion

    }
}


