using UnityEngine;

namespace Niki.UI
{
    /// <summary>
    /// View model for one ability slot.
    ///
    /// The model layer (e.g. playerUpgradeManager + UpgradeDatabase + GameInput)
    /// writes into these properties. Every widget element bound to a property
    /// updates automatically - no per-element Update() polling needed.
    /// </summary>
    public class AbilitySlotViewModel : IBindingContext
    {
        /// <summary>Ability icon (from UpgradeScriptableOBJ.IconImage).</summary>
        public readonly Property<Sprite> Icon = new();

        /// <summary>Ability name label (from UpgradeScriptableOBJ.name / itemDesc).</summary>
        public readonly Property<string> Name = new("?");

        /// <summary>
        /// Remaining cooldown, 0..1.
        /// 1 = just used (full cooldown remaining), 0 = ready.
        /// </summary>
        public readonly Property<float> CooldownRemaining = new(0f);

        /// <summary>True while the player is holding the ability input (press feedback).</summary>
        public readonly Property<bool> IsPressed = new(false);

        /// <summary>Command invoked when the slot's button is pressed. The model layer plugs the logic in via SetAction.</summary>
        public readonly Command Activate = new();
    }
}
