using UnityEngine;

namespace LordBreakerX.AbilitySystem
{
    /// <summary>
    /// Represents the base class for all abilities in the ability system.
    /// This class provides the core functionality for ability initialization, 
    /// and methods that must be implemented by derived ability classes.
    /// </summary>
    public abstract class BaseAbility : ScriptableObject, IAbility
    {
        /// <summary>
        /// The unique identifier for the ability.
        /// </summary>
        [SerializeField]
        private string _id;

        /// <summary>
        /// Gets the handler that manages the ability.
        /// </summary>
        protected AbilityHandler Handler { get; private set; }

        /// <summary>
        /// Gets the unique identifier for the ability.
        /// </summary>
        public string ID => _id;

        /// <summary>
        /// Determines if the ability can be used.
        /// </summary>
        /// <returns>True if the ability can be used; otherwise, false.</returns>
        public abstract bool CanUse();

        /// <summary>
        /// Begins the execution of the ability.
        /// </summary>
        public abstract void BeginAbility();

        /// <summary>
        /// Finishes the execution of the ability.
        /// </summary>
        public abstract void FinishAbility();

        /// <summary>
        /// Updates the ability's state on each frame.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Updates the ability's state on each fixed frame (e.g., physics update).
        /// </summary>
        public abstract void FixedUpdate();

        /// <summary>
        /// Performs any initialization tasks required for the ability. This happens when the ability is registered to an ability handler.
        /// </summary>
        protected abstract void OnInitilization();

        public void Initilize(AbilityHandler handler)
        {
            Handler = handler;
            OnInitilization();
        }

    }
}
