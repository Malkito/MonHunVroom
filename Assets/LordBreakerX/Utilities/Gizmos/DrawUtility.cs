using Newtonsoft.Json.Bson;
using UnityEngine;

namespace LordBreakerX.Utilities.Drawing
{
    public static class DrawUtility
    {
        public static void DrawBox(Vector3 center, Vector3 size, Color sideColor, Color edgeColor)
        {
            Color startingColor = Gizmos.color;

            Gizmos.color = sideColor;

            Gizmos.DrawCube(center, size);

            Gizmos.color = edgeColor;

            Gizmos.DrawWireCube(center, size);

            Gizmos.color = startingColor;
        }

        public static void DrawBox(Vector3 center, Vector3 size, Color sideColor)
        {
            DrawBox(center, size, sideColor, Color.black);
        }

        public static void DrawBox(Transform centerTransform, Vector3 centerOffset, Vector3 size, Color sideColor, Color edgeColor)
        {
            DrawBox(centerTransform.position + centerOffset, size, sideColor, edgeColor);
        }

        public static void DrawBox(Transform centerTransform, Vector3 centerOffset, Vector3 size, Color sideColor)
        {
            DrawBox(centerTransform.position + centerOffset, size, sideColor, Color.black);
        }

        public static void DrawBox(Transform centerTransform, Vector3 size, Color sideColor, Color edgeColor)
        {
            DrawBox(centerTransform.position, size, sideColor, edgeColor);
        }

        public static void DrawBox(Transform centerTransform, Vector3 size, Color sideColor)
        {
            DrawBox(centerTransform.position, size, sideColor, Color.black);
        }


    }
}
