using UnityEngine;

namespace LordBreakerX.AttackSystemNew
{
    /// <summary>
    /// used for creating an attack useable with the attack system.
    /// </summary>
    public interface IAttack
    {
        /// <summary>
        /// Called when the attack starts
        /// </summary>
        /// <param name="context"></param>
        public void OnAttackStart(AttackNetworkContext context);

        /// <summary>
        /// Called when the attack has fully stopped
        /// </summary>
        /// <param name="context"></param>
        public void OnAttackStop(AttackNetworkContext context);

        /// <summary>
        /// Called every frame, if the attack is active
        /// </summary>
        /// <param name="context"></param>
        public void OnAttackUpdate(AttackNetworkContext context);

        /// <summary>
        /// Called every fixed framerate frame, if the attack is active
        /// </summary>
        /// <param name="context"></param>
        public void OnAttackFixedUpdate(AttackNetworkContext context);

        /// <summary>
        /// Checks if the attack can currently be used
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool CanUseAttack(AttackNetworkContext context);

        /// <summary>
        /// checks if the attack has finished and is ready to end
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool HasAttackFinished(AttackNetworkContext context);
        
    }
}
