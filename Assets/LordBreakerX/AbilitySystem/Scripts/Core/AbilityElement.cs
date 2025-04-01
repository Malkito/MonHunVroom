using UnityEngine;

namespace LordBreakerX.AbilitySystem
{
    [System.Serializable]
    public struct AbilityElement
    {
        [SerializeField]
        private string _id;

        [SerializeField]
        private BaseAbility _ability;

        public string Id { get { return _id; } }

        public BaseAbility Ability { get { return _ability; } }

        public AbilityElement(string id, BaseAbility ability)
        {
            _id = id;
            _ability = ability;
        }
    }
}
