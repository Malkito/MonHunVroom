using UnityEngine;

namespace LordBreakerX.States
{
    public class StateReference : ScriptableObject
    {
        [SerializeField]
        [SerializeReference]
        private State _stateTemplate;

        public string ID { get => _stateTemplate.ID; set => _stateTemplate.ID = value;  }

        private void Reset()
        {
            if (_stateTemplate != null)
            {
                _stateTemplate.ID = name;
            }
        }

        public System.Type GetStateType()
        {
            return _stateTemplate.GetType();
        }

        public State GetState(IStateMachine stateMachine) => _stateTemplate.CreateCopy(stateMachine);

        public static StateReference Create(State state)
        {
            StateReference stateRef = CreateInstance<StateReference>();
            stateRef._stateTemplate = state;
            return stateRef;
        }

        public static explicit operator StateReference(State state)
        {
            return Create(state);
        }
    }

}