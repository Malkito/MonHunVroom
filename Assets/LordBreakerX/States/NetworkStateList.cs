using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.States.Networked
{
    [CreateAssetMenu(menuName = "State Machines/Network State List")]
    public class NetworkStateList : ScriptableObject
    {
        [SerializeField]
        private List<NetworkScriptableState> _states;

        public NetworkScriptableState GetState(string stateID)
        {
            foreach(NetworkScriptableState state in _states)
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
