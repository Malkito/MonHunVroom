using UnityEngine;

namespace LordBreakerX.States.Networked
{
    /// <summary>A state template that the network state machine can clone and run.</summary>
    public abstract class StateReference : ScriptableObject
    {
        public const string CREATE_PATH = "State Machines/States/";

        [SerializeField] private string _id;
        private bool _isEnabled;

        protected NetworkStateMachine Machine { get; private set; }
        protected GameObject MachineObject => Machine.gameObject;
        protected Vector3 Position => Machine.transform.position;
        protected bool IsServer => Machine.IsServer;
        protected bool IsHost => Machine.IsHost;
        protected bool IsClient => Machine.IsClient;
        protected bool IsOwner => Machine.IsOwner;
        public string ID => _id;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                if (value) OnStateEnabled();
                else OnStateDisabled();
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (!string.IsNullOrEmpty(_id)) return;
            var type = GetType();
            _id = string.IsNullOrEmpty(type.Namespace) ? type.Name : $"{type.Namespace}_{type.Name}";
        }
#endif

        protected internal virtual void OnCreateState() { }
        protected internal virtual void OnDestroyState() { }
        protected internal virtual void OnEnterState() { }
        protected internal virtual void OnExitState() { }
        protected internal virtual void OnUpdateState() { }
        protected internal virtual void OnFixedUpdateState() { }
        protected internal virtual void OnStateEnabled() { }
        protected internal virtual void OnStateDisabled() { }

        internal static StateReference CloneState(StateReference template, NetworkStateMachine machine)
        {
            var instance = Instantiate(template);
            instance.Machine = machine;
            return instance;
        }
    }
}
