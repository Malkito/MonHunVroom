using UnityEngine;

namespace Niki.UI
{
    public readonly struct AbilitySlotData
    {
        public readonly Sprite Icon;
        public readonly Color Color;
        public readonly string Name;
        public readonly float CooldownRemaining;
        public readonly bool IsPressed;

        public AbilitySlotData(Sprite icon, Color color, string name, float cooldownRemaining, bool isPressed)
        {
            Icon = icon;
            Color = color;
            Name = name;
            CooldownRemaining = cooldownRemaining;
            IsPressed = isPressed;
        }
    }

    public interface IPlayerAbilitySlots
    {
        bool TryGetSlot(int index, out AbilitySlotData slot);
    }
}
