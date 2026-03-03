using System.Collections.Generic;
using UnityEngine;

namespace LordBreakerX.AttackSystem
{
    public struct AttackTarget
    {
        private Transform _targetTransform;
        private Vector3 _fallbackPosition;

        private Dictionary<Transform, Collider> _colliderRegistry;

        public bool IsTargettingObject { get => _targetTransform != null; }

        public GameObject Object 
        { 
            get 
            { 
                if (IsTargettingObject) return _targetTransform.gameObject;
                else return null;
            } 
        }

        public AttackTarget(Transform targetTransform, Vector3 fallbackPosition)
        {
            _targetTransform = targetTransform;
            _fallbackPosition = fallbackPosition;
            _colliderRegistry = new Dictionary<Transform, Collider>();

            if (!IsTargettingObject || _colliderRegistry.ContainsKey(_targetTransform)) return;

            if (_targetTransform.TryGetComponent<Collider>(out Collider targetCollider))
            {
                _colliderRegistry[_targetTransform] = targetCollider;
            }
        }

        public AttackTarget(Vector3 targetPosition) : this(null, targetPosition)
        {

        }

        public Vector3 GetCenteredPosition()
        {
            if (!IsTargettingObject) return _fallbackPosition;
            if (!_colliderRegistry.ContainsKey(_targetTransform)) return _targetTransform.position;

            Collider collider = _colliderRegistry[_targetTransform];
            return collider.bounds.center;
        }

        public Vector3 GetPosition()
        {
            if (IsTargettingObject) return _targetTransform.position;
            return _fallbackPosition;
        }
    }

}