namespace LordBreakerX.AbilitySystem
{
    /// <summary>
    /// Represents an instant ability that deactivates immediately when used.
    /// </summary>
    public abstract class InstantAbility : BaseAbility
    {
        /// <summary>
        /// Called when the ability begins. 
        /// Activates the ability and immediately stops it.
        /// </summary>
        public override void BeginAbility()
        {
            ActivateAbility();
            Handler.StopAbility(ID);
        }

        /// <summary>
        /// Activates the ability's effect.
        /// </summary>
        public abstract void ActivateAbility();
    }
}
