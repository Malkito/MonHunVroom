using LordBreakerX.Stats;
using UnityEngine;

public class LevelsCompletedModifier
{
    [CustomStatModifier("Levels Completed Modifier", StatType.Float)]
    public class FloatModifier : StatModifier
    {

        [SerializeField]
        [Min(0)]
        private float _multiplierPerLevel = 1.5f;

        public override float Apply(float currentValue, float baseValue)
        {
            float fullMultiplier = _multiplierPerLevel * GameStateManager.LevelsCompleted;
            return baseValue * fullMultiplier + currentValue;
        }

        public override StatModifier Copy()
        {
            FloatModifier modifier = new FloatModifier();
            modifier._multiplierPerLevel = _multiplierPerLevel;
            return modifier;
        }
    }

    [CustomStatModifier("Levels Completed Modifier", StatType.Int)]
    public class IntModifier : StatModifier
    {
        [SerializeField]
        [Min(0)]
        private float _multiplierPerLevel = 1.5f;

        public override float Apply(float currentValue, float baseValue)
        {
            float fullMultiplier = _multiplierPerLevel * GameStateManager.LevelsCompleted;
            return (int)(baseValue * fullMultiplier + currentValue);
        }

        public override StatModifier Copy()
        {
            IntModifier modifier = new IntModifier();
            modifier._multiplierPerLevel = _multiplierPerLevel;
            return modifier;
        }
    }
}
