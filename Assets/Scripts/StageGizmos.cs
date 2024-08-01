using UnityEngine;

public static class StageGizmos
{
    public static void DrawCircle(Vector3 center, float radius, Vector3 normal, Color color, int segments = 32)
    {
        Gizmos.color = color;

        float angle = 0;
        float angleStep = 360f / segments;
        Vector3 prevPos = center + Quaternion.AngleAxis(0, normal) * Vector3.right * radius;
        for (int i = 0; i < segments + 1; i++)
        {
            Vector3 newPos = center + Quaternion.AngleAxis(angle, normal) * Vector3.right * radius;
            Gizmos.DrawLine(prevPos, newPos);
            prevPos = newPos;
            angle += angleStep;
        }
    }
}