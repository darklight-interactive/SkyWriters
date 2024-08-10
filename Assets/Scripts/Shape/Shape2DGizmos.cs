
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
public static class Shape2DGizmos
{
    public static void DrawShape2D(Shape2D shape, Color color)
    {
        if (shape == null) return;
        List<Vector3> vertices = shape.vertices.ToList();
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 start = vertices[i];
            Vector3 end = vertices[(i + 1) % vertices.Count];
            Gizmos.color = color;
            Gizmos.DrawLine(start, end);
        }
    }


}
#endif