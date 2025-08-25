using LordBreakerX.Utilities.AI;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace LordBreakerX.AttackSystem
{
    public class AttackController : NetworkBehaviour
    {
        #region Variables

        [SerializeField]
        private List<AttackTable> _attackTables = new List<AttackTable>();

        [SerializeField]
        private AttackTable _fallbackTable;

        [SerializeField]
        private float _targetOffset;

        [SerializeField]
        private float _randomAttackRadius = 30;

        private ScriptableAttack _activeAttack;

        private TargetResolver _provider;

        #endregion

        #region Properties

        public bool IsAttacking { get { return _activeAttack != null; } }

        public TargetResolver TargetProvider { get { return _provider; } }

        public bool HasTarget { get => _provider.HasTarget; }

        public Vector3 TargetPosition { get => _provider.GetTargetPosiiton(); }
        public Vector3 OffsettedTargetPosition { get => _provider.GetOffsettedTargetPosition(transform.position); }

        public ScriptableAttack ActiveAttack { get => _activeAttack; }

        #endregion

        #region Temorary Properties

        public MonsterMovementController Movement { get; private set; }

        public MonsterAttackController MonsterAttack { get; private set; }

        public Animator Animator { get; private set; }

        #endregion

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Movement = GetComponent<MonsterMovementController>();
            MonsterAttack = GetComponent<MonsterAttackController>();
            Animator = GetComponent<Animator>();

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

            AttackProgress progress = _activeAttack.GetAttackProgress(this);

            if (progress == AttackProgress.Preparing) _activeAttack.OnPreperationUpdate(this);
            else if (progress == AttackProgress.Attacking) _activeAttack.OnAttackUpdate(this);
            else
            {
                RequestStopAttack();
            }
        }

        private void FixedUpdate()
        {
            if (_activeAttack == null || !IsServer) return;

            AttackProgress progress = _activeAttack.GetAttackProgress(this);

            if (progress == AttackProgress.Preparing) _activeAttack.OnPreperationFixedUpdate(this);
            else if (progress == AttackProgress.Attacking) _activeAttack.OnAttackFixedUpdate(this);
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
            AttackTable attackTable = GetTable();

            if (IsAttacking || attackTable == null) return;

            _activeAttack = attackTable.GetRandomAttack();
            _activeAttack.OnStart(this);
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
                return useableTables[usedTableIndex];
            }

            return _fallbackTable;
        }

        [ClientRpc(RequireOwnership = false)]
        private void StartAttackClientRpc()
        {
            StartAttack();
        }

        public void RequestRandomAttack()
        {
            if (!IsServer) return;

            Vector2 random = Random.insideUnitCircle * _randomAttackRadius;
            Vector3 attackPosition = new Vector3(random.x, transform.position.y, random.y);

            if (RandomPathGenerator.IsPathValid(new NavMeshPath(), transform.position, attackPosition))
            {
                TargetProvider.SetTargetPosition(attackPosition);
                RequestStartAttack();
            }
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
            if (_activeAttack != null) _activeAttack.OnStop(this);
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
            if (_attackTables.Count > 0)
            {
                foreach (AttackTable attackTable in _attackTables)
                {
                    if (attackTable != null) attackTable.DrawGizmos(this);
                }
            }

            if (_provider != null)
            {
                _provider.DrawTarget(transform.position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_attackTables.Count > 0)
            {
                foreach (AttackTable attackTable in _attackTables)
                {
                    if (attackTable != null) attackTable.DrawGizmosSelected(this);
                }
            }
        }

        #endregion

    }
}


