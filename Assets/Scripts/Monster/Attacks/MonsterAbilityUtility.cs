using LordBreakerX.AbilitySystem;
using LordBreakerX.States;
using LordBreakerX.Utilities.Math;
using System;
using UnityEngine;

[System.Serializable]
public class MonsterAbilityUtility
{
    [SerializeField]
    [Min(0)]
    private float _targetRange;

    public MonsterController Monster { get; private set; }
    public StateMachine Machine { get; private set; }

    private Transform _monsterTransform;

    public void Initilize(AbilityHandler handler)
    {
        _monsterTransform = handler.transform;
        Monster = handler.GetComponent<MonsterController>();
        Machine = handler.GetComponent<StateMachine>();
    }

    public Vector3 GetTargetPosition()
    {
        Vector3 randomPosition = PositionUtility.GetRandomPositionInFrontHalfSquare(_targetRange, _monsterTransform.position + Monster.MonsterBottom, _monsterTransform.forward, _monsterTransform.right);
        return randomPosition;
    }
}
