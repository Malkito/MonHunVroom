using LordBreakerX.Utilities;
using LordBreakerX.Utilities.Drawing;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster/AttackConditions/Target Behind")]
public class WithinBox : AttackCondition
{
    private static readonly Color BOX_SIDE_COLOR = new Color(0.0f, 0.0f, 1.0f, 0.2f);
    private static readonly Color SPHERE_COLOR = new Color(1.0f, 0.0f, 0.0f, 0.4f);

    [SerializeField]
    private Vector3 _startOffset;

    [SerializeField]
    [Min(0)]
    private Vector3 _size;

    [SerializeField]
    private LayerMask _targetLayers;

    [SerializeField]
    private BoxPositionMode _positionMode;

    private Vector3 _halfSize;

    private Vector3 _boxCenter;

    private Vector3 _startPosition;

    private Vector3 _boxMin;
    private Vector3 _boxMax;

    public override bool CanUse(AttackController controller)
    {
        UpdateBoxInfo(controller.transform);
        Vector3 targetPosition = controller.TargetPosition;
        return targetPosition.x >= _boxMin.x && targetPosition.x <= _boxMax.x && targetPosition.y >= _boxMin.y && targetPosition.y <= _boxMax.y; 
    }

    private void UpdateBoxInfo(Transform attackerTransform)
    {
        _startPosition = attackerTransform.position + _startOffset;
        Vector3 direction  = GetDirection(attackerTransform);
        _boxCenter = _startPosition + direction.Multiply(_halfSize);
        _boxMin = _boxCenter - _halfSize;
        _boxMax = _boxCenter + _halfSize;
    }

    private Vector3 GetDirection(Transform attackerTransform)
    {
        switch (_positionMode)
        {
            case BoxPositionMode.Forward:
                return attackerTransform.forward;
            case BoxPositionMode.Backward:
                return -(attackerTransform.forward);
            case BoxPositionMode.Right:
                return attackerTransform.right;
            case BoxPositionMode.Left:
                return -(attackerTransform.right);
            case BoxPositionMode.Up:
                return attackerTransform.up;
            case BoxPositionMode.Down:
                return -(attackerTransform.up);
            default:
                return Vector3.zero;
        }
    }

    public override void DrawGizmos(AttackController controller)
    {
        
    }

    public override void DrawGizmosSelected(AttackController controller)
    {
        OnIniliization();

        UpdateBoxInfo(controller.transform);

        DrawUtility.DrawBox(_boxCenter, _size, BOX_SIDE_COLOR);

        Gizmos.color = SPHERE_COLOR;
        Gizmos.DrawSphere(_startPosition, 0.2f);
    }

    public override void OnIniliization()
    {
        _halfSize = new Vector3(_size.x / 2, _size.y / 2, _size.z / 2);
    }
}
