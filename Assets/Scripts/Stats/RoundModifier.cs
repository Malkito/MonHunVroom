using LordBreakerX.Stats;
using UnityEngine;

[CustomStatModifier("Round Modifier", StatType.Float)]
public class RoundModifier : StatModifier
{
    [SerializeField]
    [Min(0)]
    private int _maxDecimals = 2;

    public override float Apply(float currentValue, float baseValue)
    {
        return (float)System.Math.Round(currentValue, _maxDecimals);
    }

    public override StatModifier Copy()
    {
        RoundModifier modifier = new RoundModifier();
        modifier._maxDecimals = _maxDecimals;
        return modifier;
    }
}
