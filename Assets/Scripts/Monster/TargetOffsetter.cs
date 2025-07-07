using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetOffsetter
{
    // offset from the colliders edges
    private float _offsetDistance;

    private Collider _targetCollider;

    private List<Vector3> _offsets;

    public bool HasOffset { get { return _targetCollider != null; } }

    public Vector3 TargetPosition { get => _targetCollider.transform.position; }

    public TargetOffsetter(Transform target, float offsetDistance)
    {
        _targetCollider = target.GetComponent<Collider>();
        _offsetDistance = offsetDistance;
        SetOffsets();
    }

    private void SetOffsets()
    {
        if (_targetCollider == null) return;

        Vector3 maxExtends = _targetCollider.bounds.extents;
        Vector3 minExtends = -maxExtends;

        _offsets = new List<Vector3>()
        {
            new Vector3(minExtends.x - _offsetDistance, 0, 0),
            new Vector3(minExtends.x - _offsetDistance, 0, minExtends.z - _offsetDistance),
            new Vector3(minExtends.x - _offsetDistance, 0, maxExtends.z + _offsetDistance),
            new Vector3(0, 0, minExtends.z - _offsetDistance),
            new Vector3(maxExtends.x + _offsetDistance, 0, 0),
            new Vector3(maxExtends.x + _offsetDistance, 0, minExtends.z - _offsetDistance),
            new Vector3(maxExtends.x + _offsetDistance, 0, maxExtends.z + _offsetDistance),
            new Vector3(0, 0, maxExtends.z + _offsetDistance)
        };
    }

    public Vector3 GetOffsettedPosition(Vector3 startPosition)
    {
        if (HasOffset)
        {
            foreach (Vector3 offsetPoint in _offsets)
            {
                if (IsPathValid(offsetPoint, startPosition)) return offsetPoint;
            }
        }

        return startPosition;
    }

    public bool IsValidTarget(Vector3 startPosition)
    {
        if (!HasOffset) return true;

        foreach (Vector3 offsetPoint in _offsets) 
        {
            if (IsPathValid(offsetPoint, startPosition)) return true;
        }

        return false;
    }

    public void DrawPoints(Vector3 startPosition)
    {
        Color startingColor = Gizmos.color;

        if (!HasOffset) return;

        foreach(Vector3 offsetPoint in _offsets)
        {
            if (IsPathValid(offsetPoint, startPosition)) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;

            Gizmos.DrawSphere(TargetPosition + offsetPoint, 0.1f);
        }

        Gizmos.color = startingColor;
    }

    private bool IsPathValid(Vector3 point, Vector3 startPosition)
    {
        NavMeshPath path = new NavMeshPath();

        if (NavMesh.CalculatePath(startPosition, TargetPosition + point, NavMesh.AllAreas, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }
}
