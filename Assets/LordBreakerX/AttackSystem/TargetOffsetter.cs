using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetOffsetter
{
    private float _offset;

    private Collider _targetCollider;

    private List<Vector3> _offsettedPositions;

    public bool HasOffset { get { return _targetCollider != null; } }

    public TargetOffsetter(Transform target, float offset)
    {
        _targetCollider = target.GetComponent<Collider>();
        _offset = offset;
    }

    private void UpdateOffsettedPoints(Vector3 startPosition)
    {
        Vector3 max = _targetCollider.bounds.max;
        Vector3 min = _targetCollider.bounds.min;

        _offsettedPositions = new List<Vector3>()
        {
           new Vector3(min.x - _offset, startPosition.y, _targetCollider.transform.position.z),
           new Vector3(_targetCollider.transform.position.x, startPosition.y, min.z - _offset),
           new Vector3(max.x + _offset, startPosition.y, _targetCollider.transform.position.z),
           new Vector3(_targetCollider.transform.position.x, startPosition.y, max.z + _offset),
        };
    }

    public Vector3 GetOffsettedPosition(Vector3 startPosition)
    {
        if (HasOffset)
        {
            UpdateOffsettedPoints(startPosition);

            foreach (Vector3 offsetPoint in _offsettedPositions)
            {
                if (IsPathValid(offsetPoint, startPosition)) return offsetPoint;
            }
        }

        return startPosition;
    }

    public bool IsValidTarget(Vector3 startPosition)
    {
        if (!HasOffset) return true;

        UpdateOffsettedPoints(startPosition);

        foreach (Vector3 offsetPoint in _offsettedPositions)
        {
            if (IsPathValid(offsetPoint, startPosition)) return true;
        }

        return false;
    }

    public void DrawPoints(Vector3 startPosition)
    {
        Color startingColor = Gizmos.color;

        if (!HasOffset) return;

        UpdateOffsettedPoints(startPosition);

        foreach (Vector3 offsetPoint in _offsettedPositions)
        {
            if (IsPathValid(offsetPoint, startPosition)) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;

            Gizmos.DrawSphere(offsetPoint, 0.1f);
        }

        Gizmos.color = startingColor;
    }

    private bool IsPathValid(Vector3 point, Vector3 startPosition)
    {
        NavMeshPath path = new NavMeshPath();

        if (NavMesh.CalculatePath(startPosition, point, NavMesh.AllAreas, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }
}