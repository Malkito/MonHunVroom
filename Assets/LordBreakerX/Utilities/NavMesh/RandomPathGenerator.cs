using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace LordBreakerX.Utilities.AI
{
    public class RandomPathGenerator
    {
        private float _minRadius;
        private float _maxRadius;
        private float _decreaseRate;
        private Transform _agentTransform;

        public bool IsFindingPath {  get; private set; }
        public NavMeshPath GeneratedPath { get; private set; }
        public Vector3 GeneratedDestination { get; private set; }

        public RandomPathGenerator(Transform agentTransform, float minRadius, float maxRadius, float decreaseRate)
        {
            _minRadius = minRadius;
            _maxRadius = maxRadius;
            _decreaseRate = decreaseRate;
            _agentTransform = agentTransform;
        }

        public RandomPathGenerator(Transform agentTransform, float radius) : this(agentTransform, radius, radius, 0)
        {
        }

        public IEnumerator FindReachablePath()
        {
            if (!IsFindingPath)
            {
                IsFindingPath = true;
                float currentRadius = _maxRadius;
                Vector3 protentialDestination = NavMeshUtility.GetRandomPosition(_agentTransform.position, currentRadius);

                GeneratedPath = new NavMeshPath();

                while (!IsPathValid(GeneratedPath, _agentTransform.position, protentialDestination))
                {
                    currentRadius = Mathf.Clamp(currentRadius - _decreaseRate, _minRadius, _maxRadius);
                    protentialDestination = NavMeshUtility.GetRandomPosition(_agentTransform.position, currentRadius);
                    yield return null;
                }

                IsFindingPath = false;
                GeneratedDestination = protentialDestination;
            }
        }

        public static bool IsPathValid(NavMeshPath path, Vector3 startPosition, Vector3 targetPosition)
        {
            if (NavMesh.CalculatePath(startPosition, targetPosition, NavMesh.AllAreas, path))
            {
                return path.status == NavMeshPathStatus.PathComplete;
            }
            return false;
        }
    }
}
