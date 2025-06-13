using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtility
{
    public static Vector3 GetRandomPosition(Vector3 currentPosition, float radius)
    {
        Vector3 randomPoint = (Random.insideUnitSphere * radius) + currentPosition;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return currentPosition;
        }
    }
}
