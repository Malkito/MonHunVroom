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

    public TargetOffsetter CurrentOffsetter { get { return _targetOffsetters[_targetTransform]; } }

    public void SetTarget(Transform targetTransform, Vector3 fallbackPosition)
    {
        _targetTransform = targetTransform;

        if (targetTransform != null && !_targetOffsetters.ContainsKey(targetTransform))
            _targetOffsetters[targetTransform] = new TargetOffsetter(_targetTransform, _offset);

        _fallbackPosition = fallbackPosition;
    }

    public void SetTarget(Vector3 targetPosition)
    {
        SetTarget(null, targetPosition);
    }

    public Vector3 GetPosiiton()
    {
        if (HasTarget) return _targetTransform.position;
        return _fallbackPosition;
    }

    public Vector3 GetOffsettedPosition(Vector3 startPosition)
    {
        if (HasTarget) return GetTransformOffset(startPosition);
        return _fallbackPosition;
    }

    private Vector3 GetTransformOffset(Vector3 startPosition)
    {
        if (!CurrentOffsetter.HasOffset) return _targetTransform.position;
        return CurrentOffsetter.GetOffsettedPosition(startPosition);
    }

    public bool IsValidTarget(Vector3 startPosition)
    {
        return _targetTransform == null || CurrentOffsetter.IsValidTarget(startPosition);
    }

    #region Drawing

    public void DrawTarget(Vector3 startPosition)
    {
        Color startColor = Gizmos.color;

        if (!HasTarget)
        {
            DrawPosition(_fallbackPosition);
            return;
        }

        if (!CurrentOffsetter.HasOffset)
        {
            DrawPosition(_targetTransform.position);
            return;
        }

        CurrentOffsetter.DrawPoints(startPosition);

        Gizmos.color = startColor;
    }

    private void DrawPosition(Vector3 startPosition)
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(startPosition, 0.1f);
    }

    #endregion
}
