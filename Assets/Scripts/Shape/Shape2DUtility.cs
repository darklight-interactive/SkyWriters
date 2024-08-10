using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

public static class Shape2DUtility
{

    #region ================ [[ RADIAL ]] ================ >>
    public static Vector3[] GenerateRadialPoints(Vector3 center, float radius, int count, Vector3 normal)
    {
        List<Vector3> positions = new List<Vector3>();

        // Foreach step in the circle, calculate the points
        float angleStep = 360.0f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector3 newPoint = center + Quaternion.AngleAxis(angle, normal) * Vector3.right * radius;
            positions.Add(newPoint);
        }
        return positions.ToArray();
    }

    public static Vector3 CalculateAntipodalPoint(Vector3 center, Vector3 point)
    {
        Vector3 directionXZ = point - center; // Get the direction vector from the center to the point
        Vector3 antipodalPoint = center - directionXZ; // Get the antipodal point by reversing the direction vector
        return new Vector3(antipodalPoint.x, center.y, antipodalPoint.z);
    }
    #endregion

}