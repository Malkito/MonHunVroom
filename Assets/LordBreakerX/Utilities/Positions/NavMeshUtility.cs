using UnityEngine;
using UnityEngine.AI;

namespace LordBreakerX.Utilities
{
    public static class NavMeshUtility
    {
        public static Vector3 GetRandomPosition(this Transform agentTransform, float radius, int areaMask = NavMesh.AllAreas)
        {
            return GetRandomPosition(agentTransform.position, radius, NavMesh.AllAreas);
        }

        public static Vector3 GetRandomPosition(Vector3 currentPosition, float radius, int areaMask = NavMesh.AllAreas)
        {
            float x = Random.Range(-radius, radius);
            float z = Random.Range(-radius, radius);
            Vector3 randomPoint = currentPosition + new Vector3(x, 0, z);

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, areaMask))
            {
                return hit.position;
            }
            else
            {
                return currentPosition;
            }
        }

        public static bool IsPathValid(this Transform agentTransform, Vector3 targetPosition, bool partialAllowed = false, int areaMask = NavMesh.AllAreas)
        {
            return IsPathValid(agentTransform.position, targetPosition, partialAllowed);
        }

        public static bool IsPathValid(Vector3 startPosition, Vector3 targetPosition, bool partialAllowed = false, int areaMask = NavMesh.AllAreas)
        {
            NavMeshPath path = new NavMeshPath();

            if (NavMesh.CalculatePath(startPosition, targetPosition, areaMask, path))
            {
                return path.IsPathValid(partialAllowed);
            }
            return false;
        }

        public static bool IsPathValid(this NavMeshPath path, bool partialAllowed = false)
        {
            return path.status == NavMeshPathStatus.PathComplete || (partialAllowed && path.status == NavMeshPathStatus.PathPartial);
        }

        public static bool ReachedDestination(this NavMeshAgent agent, float reachedDistance)
        {
            Vector3 destination = new Vector3(agent.destination.x, agent.transform.position.y, agent.destination.z);
            return Vector3.Distance(destination, agent.transform.position) <= reachedDistance;
        }

        public static bool ReachedDestination(this NavMeshAgent agent)
        {
            return ReachedDestination(agent, agent.stoppingDistance);
        }

        public static void SetRandomDestination(this NavMeshAgent agent, float radius, int areaMask = NavMesh.AllAreas)
        {
            Vector3 destination = agent.transform.GetRandomPosition(radius, areaMask);
            agent.SetDestination(destination);
        }

        public static void SetRandomDestination(this NavMeshAgent agent, Vector3 middlePoint, float radius, int areaMask = NavMesh.AllAreas) 
        {
            Vector3 destination = GetRandomPosition(middlePoint, radius, areaMask);
            agent.SetDestination(destination);
        }

        public static void StopAgent(this NavMeshAgent agent) 
        {
            agent.SetDestination(agent.transform.position);
        }
    }
}
