using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public class AttackTarget
    {
        private Transform _targetTransform;
        private Vector3 _fallbackPosition;

        private Dictionary<Transform, Collider> _colliderRegistry = new Dictionary<Transform, Collider>();

        public bool IsTargettingObject { get => _targetTransform != null; }

        public void Set(Transform targetTransform, Vector3 fallbackPosition)
        {
            _targetTransform = targetTransform;
            _fallbackPosition = fallbackPosition;

            if (!IsTargettingObject || _colliderRegistry.ContainsKey(_targetTransform)) return;

            if (_targetTransform.TryGetComponent<Collider>(out Collider targetCollider))
            {
                _colliderRegistry[_targetTransform] = targetCollider;
            }
        }

        public void Set(Vector3 targetPosition)
        {
            Set(null, targetPosition);
        }

        public Vector3 GetCenteredTargetPosition()
        {
            if (!IsTargettingObject) return _fallbackPosition;
            if (!_colliderRegistry.ContainsKey(_targetTransform)) return _targetTransform.position;

            Collider collider = _colliderRegistry[_targetTransform];
            return collider.bounds.center;
        }

        public Vector3 GetTargetPosition()
        {
            if (IsTargettingObject) return _targetTransform.position;
            return _fallbackPosition;
        }
    }

}