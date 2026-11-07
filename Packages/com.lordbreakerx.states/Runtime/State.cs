using UnityEngine;

namespace LordBreakerX.States
{
    [System.Serializable]
    public abstract class State
    {
        [SerializeField]
        private string _id;

        public string ID { get => _id; set => _id = value; }

        /// <summary>
        /// The state machine that controls this state
        /// </summary>
        protected IStateMachine Machine { get; private set; }

        protected GameObject ParentObject { get => Machine.ParentObject; }

        protected Transform ParentTransform { get => Machine.ParentTransform; }

        /// <summary>
        /// The position of the owner of this state and the contained state machine.
        /// </summary>
        protected Vector3 Position { get => Machine.Position; }

        public State()
        {

        }

        public State(IStateMachine machine, State stateToCopy)
        {
            _id = stateToCopy.ID;
            Machine = stateToCopy.Machine;
        }

        /// <summary>
        /// Called once when the state is added to a state machine.
        /// </summary>
        public virtual void OnCreate() { }

        /// <summary>
        /// Called once when the state is removed from a state machine.
        /// </summary>
        public virtual void OnDestroy() { }

        /// <summary>
        /// Called whenever the state becomes the current state
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// Called whenever this state becomes no longer the current state
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// Called every frame while the state is the current state
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Called after every fixed frame rate while the state is the current state
        /// </summary>
        public virtual void OnFixedUpdate() { }

        public abstract State CreateCopy(IStateMachine machine);

        public sealed override bool Equals(object obj)
        {
            if (obj is State state)
            {
                return Equals(state);
            }

            return false;
        }

        private bool Equals(State state)
        {
            return !state.Equals(null) && state.ID.Equals(ID);
        }

        public sealed override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return ID;
        }
    }

}