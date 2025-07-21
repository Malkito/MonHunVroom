using UnityEngine;

[System.Serializable]
public struct AttackEntry
{
    [SerializeField]
    private Attack _attack;

    [SerializeField]
    [Min(1)]
    private int _weight;

    public Attack ChoosenAttack { get => _attack; }

    public int Weight { get => _weight; }

    public AttackEntry(Attack attack, int weight)
    {
        _attack = attack;
        _weight = weight;
    }
}
