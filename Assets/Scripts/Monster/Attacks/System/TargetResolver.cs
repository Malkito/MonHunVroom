using UnityEngine;

public class TargetResolver
{
    private Transform _targetTransform;
    private Vector3 _fallbackPosition;

    public bool HasTarget { get => _targetTransform != null; }

    /// <summary>
    ///  Set the target using the transform of the target and a fallback position in case the target transform is null.
    /// </summary>
    public void SetTarget(Transform targetTransform, Vector3 fallbackPosition) 
    {
        _targetTransform = targetTransform;
        _fallbackPosition = fallbackPosition;
    }

    /// <summary>
    /// Sets the target position. This will set the target transform to null automatically
    /// </summary>
    public void SetTargetPosition(Vector3 targetPosition)
    {
        _targetTransform = null;
        _fallbackPosition = targetPosition;
    }

    /// <summary>
    /// Gets the target position using either an transform of the fallback position.
    /// </summary>
    public Vector3 GetTargetPosiiton()
    {
        if (HasTarget) return _targetTransform.position;
        return _fallbackPosition;
    }

}
