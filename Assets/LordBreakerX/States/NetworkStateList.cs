using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.States.Networked
{
    [CreateAssetMenu(menuName = "State Machines/Network State List")]
    public class NetworkStateList : ScriptableObject
    {
        [SerializeField]
        private List<StateReference> _states;

        public StateReference GetState(string stateID)
        {
            foreach(StateReference state in _states)
            {
                if (state.ID == stateID)
                {
                    return state;
                }
            }

            return null;
        }
    }
}
