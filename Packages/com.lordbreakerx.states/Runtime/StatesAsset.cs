using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.States
{
    [CreateAssetMenu()]
    public class StatesAsset : ScriptableObject
    {
        [SerializeField]
        private List<StateReference> _states = new List<StateReference>();

        [SerializeField]
        private StateReference _startingState;

        public List<StateReference> States { get => _states; }

        public StateReference StartingState { get => _startingState; set => _startingState = value;  }

        public State[] CreateStates(IStateMachine stateMachine)
        {
            State[] createdStates = new State[_states.Count];

            for (int i = 0; i < _states.Count; i++)
            {
                createdStates[i] = _states[i].GetState(stateMachine);
            }

            return createdStates;
        }
    }

}