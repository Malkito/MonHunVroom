using LordBreakerX.Stats;
using LordBreakerX.Utilities;
using UnityEngine;

[CustomStatModifier("Player Count Modifier", StatType.Int)]
public class IntPlayerCountModifier : StatModifier
{
    [SerializeField]
    [Min(1)]
    private int _maxPlayers = 4;

    [SerializeField]
    private int _minOffset;

    [SerializeField]
    private int _maxOffset;

    public override float Apply(float currentValue, float baseValue)
    {
        playerHealth[] players = Object.FindObjectsByType<playerHealth>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        float playerPercentage = Percentage.Create(players.Length, 0, _maxPlayers);

        return (int)(currentValue + Percentage.MapToNumber(playerPercentage, _minOffset, _maxOffset));
    }

    public override StatModifier Copy()
    {
        IntPlayerCountModifier modifier = new IntPlayerCountModifier();
        return modifier;
    }
}
