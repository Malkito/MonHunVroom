using UnityEngine;

namespace LordBreakerX.States.Networked
{
    /// <summary>
    /// a wrapper class for making a reference to a state that can be easily used in the inspector.
    /// </summary>
    public class StateReference : ScriptableObject
    {
        public const string CREATE_PATH = "State Machines/States/";

        [SerializeField]
        private State _stateTemplate;

        //public State CreateState(NetworkStateMachine machine)
        //{
        //     _stateTemplate.CreateInstance(machine);
        //}
    }
}
