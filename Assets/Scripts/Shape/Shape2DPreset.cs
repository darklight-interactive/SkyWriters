using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Darklight/Shape2D/Preset")]
public class Shape2DPreset : ScriptableObject
{
    [SerializeField, Range(1, 3000)] int _radius;
    [SerializeField, Range(2, 64)] int _segments;

    public Shape2D CreateShapeAt(Vector3 center)
    {
        return new Shape2D(center, _radius, _segments);
    }

}