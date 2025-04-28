using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtility
{
    public static Vector3 GetRandomPosition(Vector3 currentPosition, float range)
    {
        Vector3 randomPoint = (Random.insideUnitSphere * range) + currentPosition;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return currentPosition;
        }
    }
}
