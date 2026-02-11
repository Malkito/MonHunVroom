using LordBreakerX.AttackSystem;
using LordBreakerX.Utilities;
using UnityEngine;


[CreateAssetMenu(menuName = "Attacks/Throw Attack")]
public class ThrowAttack : ScriptableAttack
{
    [SerializeField]
    private LayerMask _ignoredLayers;

    [SerializeField]
    [Range(0f, 100f)]
    private float _throwTargetChance = 50;

    [SerializeField]
    [Min(0f)]
    private float _maxThrowDistance = 100f;

    [SerializeField]
    private float _maxPickupDistance = 100f;

    private GameObject _objectToThrow;

    private AttackTarget _throwTarget;

    public override void OnAttackStarted()
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
