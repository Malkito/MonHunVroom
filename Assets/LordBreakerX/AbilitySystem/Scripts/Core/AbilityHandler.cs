using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AbilitySystem
{
    public class AbilityHandler : MonoBehaviour
    {
        [SerializeField]
        private BaseAbility[] _abilities;

        private Dictionary<string, IAbility> _abilityRegistry = new Dictionary<string, IAbility>();

        private Dictionary<string, IAbility> _activeAbilities = new Dictionary<string, IAbility>();

        private List<string> _stopQueue = new List<string>();

        private List<IAbility> _registeredAbilities = new List<IAbility>();

        public bool HasActiveAbility { get { return _activeAbilities != null && _activeAbilities.Count > 0; } }
        public IReadOnlyList<IAbility> RegisteredAbilities { get { return _registeredAbilities; } }

        private void Awake()
        {
            foreach(BaseAbility ability in _abilities)
            {
                RegisterAbility(ability);
            }
        }

        private void Update()
        {
            if (_activeAbilities.Count > 0)
            {
                foreach (IAbility ability in _activeAbilities.Values)
                {
                    ability.Update();
                }
            }

            if (_stopQueue.Count > 0)
            {
                foreach (string ID in _stopQueue)
                {
                    IAbility ability = _activeAbilities[ID];
                    ability.FinishAbility();
                    _activeAbilities.Remove(ID);
                }

                _stopQueue.Clear();
            }
        }

        private void FixedUpdate()
        {
            if (_activeAbilities.Count > 0)
            {
                foreach (IAbility ability in _activeAbilities.Values)
                {
                    ability.FixedUpdate();
                }
            }
        }

        public void RegisterAbility(IAbility ability)
        {
            if (ability == null) return;

            if (_abilityRegistry.ContainsKey(ability.ID))
            {
                Debug.LogWarning($"Failed to register ability '{ability}' with ID '{ability.ID}' because the ID is already in use.");
            }
            else
            {
                ability.Initilize(this);
                _abilityRegistry.Add(ability.ID, ability);
                _registeredAbilities.Add(ability);
            }
        }

        public void RegisterAbility(BaseAbility ability) 
        {
            if (ability == null) return;
            IAbility copiedAbility = Instantiate(ability);
            RegisterAbility(copiedAbility);
        }

        public void StartAbility(string abilityID)
        {
            if (_abilityRegistry.ContainsKey(abilityID))
            {
                IAbility ability = _abilityRegistry[abilityID];
                if (ability.CanUse())
                {
                    _activeAbilities.Add(abilityID, ability);
                    ability.BeginAbility();
                } 
            }
            else
            {
                Debug.LogError($"Failed to start ability with ID '{abilityID}' because the ID is not in use");
            }
        }

        public void StartAbility(IAbility ability)
        {
            if (ability != null) StartAbility(ability.ID);
        }

        public void StopAbility(string abilityID) 
        {
            if (_activeAbilities.ContainsKey(abilityID))
            {
                _stopQueue.Add(abilityID);
            }
        }

        public void StopAbility(IAbility ability)
        {
            if (ability != null) StopAbility(ability.ID);
        }

        public void StopAllAbilities()
        {
            foreach(IAbility ability in _activeAbilities.Values)
            {
                if (ability != null) StopAbility(ability.ID);
            }
        }

        public void StartRandomAbility()
        {
            int attackIndex = Random.Range(0, RegisteredAbilities.Count);
            StartAbility(RegisteredAbilities[attackIndex]);
        }

    }
}
