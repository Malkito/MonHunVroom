using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtility
{
    private const string NAVMESH_OBSTACLE_MENU_PATH = "CONTEXT/NavMeshObstacle/";

    private const string INITLIZE_FROM_COLLIDER_MENU_NAME = NAVMESH_OBSTACLE_MENU_PATH + "Initlize From Collider";

    private const string INITLIZE_FROM_MESH_MENU_NAME = NAVMESH_OBSTACLE_MENU_PATH + "Initlize From Mesh";

    [MenuItem(INITLIZE_FROM_COLLIDER_MENU_NAME)]
    private static void InitlizeFromCollider(MenuCommand command)
    {
        NavMeshObstacle obstacle = (NavMeshObstacle)command.context;

        if (obstacle == null) return;

        BoxCollider collider = obstacle.GetComponentInChildren<BoxCollider>();

        if (collider != null)
        {
            Undo.RecordObject(obstacle, "Initlize Values From Collider");

            obstacle.center = collider.center;
            obstacle.size = collider.size;
            obstacle.shape = NavMeshObstacleShape.Box;

            EditorUtility.SetDirty(obstacle);
        }
    }

    [MenuItem(INITLIZE_FROM_MESH_MENU_NAME)]
    private static void InitlizeFromMesh(MenuCommand command)
    {
        NavMeshObstacle obstacle = (NavMeshObstacle)command.context;

        if (obstacle == null) return;

        MeshFilter meshFilter = obstacle.GetComponentInChildren<MeshFilter>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Undo.RecordObject(obstacle, "Initlize Values From Mesh");

            obstacle.center = meshFilter.sharedMesh.bounds.center;
            obstacle.size = meshFilter.sharedMesh.bounds.size;
            obstacle.shape = NavMeshObstacleShape.Box;

            EditorUtility.SetDirty(obstacle);
        }
    }
}
