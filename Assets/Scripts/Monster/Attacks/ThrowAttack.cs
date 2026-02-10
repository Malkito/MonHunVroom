using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;

public class ThrowAttack : Attack
{
    [SerializeField]
    private LayerMask _ignoredLayers;

    [SerializeField]
    [Range(0f, 100f)]
    private float _throwTargetChance = 50;

    [SerializeField]
    [Min(0f)]
    private float _maxThrowDistance = 100f;

    private GameObject _objectToThrow;

    public ThrowAttack(AttackController controller) : base(controller)
    {
    }

    public override Attack Clone(AttackController controller)
    {
        ThrowAttack clone = new ThrowAttack(controller);
        return clone;
    }

    public override void OnStart()
    {
        // determines if throwing target
        if (Controller.Target.IsTargettingObject && Probability.IsSuccessful(_throwTargetChance))
        {
            _objectToThrow = Controller.Target.TargetObject.gameObject;
        }
        else
        {

        }
        
    }

    public override void OnAttackUpdate()
    {
        
    }

    public override bool HasAttackFinished()
    {
        return base.HasAttackFinished();
    }
}
