using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

[System.Serializable]
public class Shape2D
{
    public const int MAX_SEGMENTS = 32;

    Vector3[] _vertices;
    public Vector3[] vertices => _vertices;


    [SerializeField, ShowOnly] Vector3 _center = Vector3.zero;
    [SerializeField, ShowOnly] float _radius = 64;
    [SerializeField, ShowOnly] int _segments = 16;
    [SerializeField, ShowOnly] Vector3 _normal = Vector3.up;

    public Vector3 center { get => _center; set => _center = value; }
    public float radius { get => _radius; set => _radius = value; }
    public int segments { get => _segments; set => _segments = value; }
    public Vector3 normal { get => _normal; set => _normal = value; }

    public Shape2D()
    {
        _vertices = Shape2DUtility.GenerateRadialPoints(_center, _radius, _segments, _normal);
    }

    public Shape2D(Vector3 center, float radius, int segments)
    {
        _center = center;
        _radius = radius;

        if (segments > MAX_SEGMENTS) segments = MAX_SEGMENTS;
        _segments = segments;

        _vertices = Shape2DUtility.GenerateRadialPoints(_center, _radius, _segments, _normal);
    }

    public Shape2D(Vector3 center, float radius, int segments, Vector3 normal)
    {
        _center = center;
        _radius = radius;

        if (segments > MAX_SEGMENTS) segments = MAX_SEGMENTS;
        _segments = segments;

        _normal = normal;

        _vertices = Shape2DUtility.GenerateRadialPoints(_center, _radius, _segments, _normal);
    }

    public bool IsPositionWithinRadius(Vector3 position)
    {
        return Vector3.Distance(_center, position) <= _radius;
    }

    public void DrawGizmos(Color color)
    {
        Shape2DGizmos.DrawShape2D(this, color);
    }

}