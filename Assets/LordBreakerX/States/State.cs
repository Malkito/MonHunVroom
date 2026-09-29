using UnityEngine;

namespace LordBreakerX.States.Networked
{
    /// <summary>
    /// a class for creating states from scriptable objects for netcode for gameobjects
    /// </summary>
    public abstract class State
    {
        [SerializeField]
        private string _id;

        private NetworkStateMachine _machine;

        public string ID { get => _id; internal set => _id = value; }

        /// <summary>
        /// The netcode state machine that controls this state
        /// </summary>
        protected NetworkStateMachine Machine { get => _machine; }

        /// <summary>
        /// The game object that owns this state and contains a state machine.
        /// </summary>
        protected GameObject ParentObject { get => _machine.gameObject; }

        /// <summary>
        /// The position of the owner of this state and the contained state machine.
        /// </summary>
        protected Vector3 Position { get => _machine.transform.position; }

        protected bool IsServer { get => _machine.IsServer; }
        protected bool IsHost { get => _machine.IsHost; }
        protected bool IsClient { get => _machine.IsClient; }
        protected bool IsOwner { get => _machine.IsOwner; }

        public State(State toInstance, NetworkStateMachine stateMachine)
        {
            toInstance._id = toInstance.ID;
            toInstance._machine = stateMachine;
        }

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

        protected internal abstract State CreateInstance(NetworkStateMachine machine);
    }
}
