using UnityEngine;

namespace LordBreakerX.States.Networked
{
    /// <summary>
    /// a class for creating states from scriptable objects for netcode for gameobjects
    /// </summary>
    public abstract class NetworkScriptableState : ScriptableObject
    {
        public const string CREATE_PATH = "State Machines/Networked States/";

        [SerializeField]
        private string _id;

        private bool _isEnabled;

        internal string ID { get => _id; }

        /// <summary>
        /// Is the state currently enabled allowing the state to be able 
        /// to be used with the state machine.
        /// </summary>
        public bool IsEnabled
        {
            get 
            { 
                return _isEnabled; 
            }
            set
            {
                _isEnabled = value;
                if (_isEnabled) OnStateEnabled();
                else OnStateDisabled();
            }
        }

        /// <summary>
        /// The netcode state machine that controls this state
        /// </summary>
        protected NetworkStateMachine Machine { get; private set; }

        /// <summary>
        /// The game object that owns this state and contains a state machine.
        /// </summary>
        protected GameObject MachineObject { get => Machine.gameObject; }

        /// <summary>
        /// The position of the owner of this state and the contained state machine.
        /// </summary>
        protected Vector3 Position { get => Machine.transform.position; }

        protected bool IsServer { get => Machine.IsServer; }
        protected bool IsHost { get => Machine.IsHost; }
        protected bool IsClient { get => Machine.IsClient; }
        protected bool IsOwner { get => Machine.IsOwner; }


#if UNITY_EDITOR
        private void Reset()
        {
            if (string.IsNullOrEmpty(_id))
            {
                System.Type type = GetType();

                string classNamespace = type.Namespace;
                string className = type.Name;

                if (string.IsNullOrEmpty(classNamespace))
                    _id = className;
                else
                    _id = $"{classNamespace}_{className}";
            }
        }
#endif

        /// <summary>
        /// Called once when the state is added to a state machine.
        /// </summary>
        protected internal virtual void OnCreateState() { }

        /// <summary>
        /// Called once when the state is removed from a state machine.
        /// </summary>
        protected internal virtual void OnDestroyState() { }

        /// <summary>
        /// Called whenever the state becomes the current state
        /// </summary>
        protected internal virtual void OnEnterState() { }

        /// <summary>
        /// Called whenever this state becomes no longer the current state
        /// </summary>
        protected internal virtual void OnExitState() { }

        /// <summary>
        /// Called every frame while the state is the current state
        /// </summary>
        protected internal virtual void OnUpdateState() { }

        /// <summary>
        /// Called after every fixed frame rate while the state is the current state
        /// </summary>
        protected internal virtual void OnFixedUpdateState() { }

        /// <summary>
        /// Called whenever the state machine is enabled.
        /// </summary>
        protected internal virtual void OnStateEnabled() { }

        /// <summary>
        /// Called whenever the state machine is disabled.
        /// </summary>
        protected internal virtual void OnStateDisabled() { }

        internal static NetworkScriptableState CloneState(NetworkScriptableState stateTemplate, NetworkStateMachine machine)
        {
            NetworkScriptableState stateInstance = Instantiate(stateTemplate);
            stateInstance.Machine = machine;
            return stateInstance;
        }
    }
}
