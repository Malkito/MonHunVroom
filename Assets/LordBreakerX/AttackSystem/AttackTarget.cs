using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public class AttackTarget
    {
        private Transform _targetTransform;
        private Vector3 _fallbackPosition;

        private Dictionary<Transform, Collider> _colliderRegistry;

        public bool IsTargettingObject { get => _targetTransform != null; }

        public Vector3 TargetPosition
        {
            get
            {
                if (IsTargettingObject) return _targetTransform.position;
                return _fallbackPosition;
            }
        }

        public void Set(Transform targetTransform, Vector3 fallbackPosition)
        {
            _targetTransform = targetTransform;

            if (IsTargettingObject && !_colliderRegistry.ContainsKey(_targetTransform))
            {
                Collider collider = _targetTransform.GetComponent<Collider>();
                if (collider != null)
                    _colliderRegistry[_targetTransform] = collider;
            }

            _fallbackPosition = fallbackPosition;
        }

        public void Set(Vector3 targetPosition)
        {
            Set(null, targetPosition);
        }

        public Vector3 GetCenteredTargetPosition()
        {
            if (IsTargettingObject && _colliderRegistry.ContainsKey(_targetTransform))
            {
                Collider collider = _colliderRegistry[_targetTransform];
                return collider.bounds.center;
            }

            return _fallbackPosition;
        }
    }

}