using System.Collections.Generic;
using UnityEngine;

public class TargetResolver
{
    private Transform _targetTransform;
    private Vector3 _fallbackPosition;

    private Dictionary<Transform, TargetOffsetter> _targetOffsetters = new Dictionary<Transform, TargetOffsetter>();

    private float _offset;

    public TargetResolver(float offset)
    {
        _offset = offset;
    }

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

    public Vector3 GetOffsettedTargetPosition(Vector3 startPosition)
    {
        if (HasTarget) return GetTransformOffset(startPosition);
        return _fallbackPosition;
    }

    private Vector3 GetTransformOffset(Vector3 startPosition)
    {
        TargetOffsetter offsetter = GetTargetOffsetter(_targetTransform);

        if (!offsetter.HasOffset) return _targetTransform.position;

        return offsetter.GetOffsettedPosition(startPosition);
    }

    private TargetOffsetter GetTargetOffsetter(Transform checkTransform) 
    {
        if (_targetOffsetters.ContainsKey(checkTransform))
        {
            return _targetOffsetters[checkTransform];
        }
        else
        {
            TargetOffsetter offsetter = new TargetOffsetter(checkTransform, _offset);
            _targetOffsetters.Add(checkTransform, offsetter);
            return offsetter;
        }
    }

    public bool IsValidTarget(Transform checkTransform, Vector3 startPosition)
    {
        TargetOffsetter targetOffsetter = GetTargetOffsetter(checkTransform);
        return targetOffsetter.IsValidTarget(startPosition);
    }

    public void DrawTarget(Vector3 startPosition)
    {
        Color startColor = Gizmos.color;

        if (!HasTarget)
        {
            DrawPosition(_fallbackPosition);
            return;
        }

        TargetOffsetter offsetter = GetTargetOffsetter(_targetTransform);

        if (!offsetter.HasOffset)
        {
            DrawPosition(_targetTransform.position);
            return;
        }

        offsetter.DrawPoints(startPosition);

        Gizmos.color = startColor;
    }

    private void DrawPosition(Vector3 position)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(position, 0.1f);
    }
}
