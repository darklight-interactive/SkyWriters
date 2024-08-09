using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Darklight/VFX/ColorData")]
public class VFX_ColorData : ScriptableObject
{
    [SerializeField] string _name = "null";
    [SerializeField] Color _color = new Color(0, 0, 0, 0);

    [HorizontalLine]
    [Header("Color Values")]
    [SerializeField, Range(0f, 1f)] float _red = 0f;
    [SerializeField, Range(0f, 1f)] float _green = 0f;
    [SerializeField, Range(0f, 1f)] float _blue = 0f;
    [SerializeField, Range(0f, 1f)] float _alpha = 1f;

    // ----------- Public Properties -----------
    public string Name => _name;
    public Color Color => _color;

    public VFX_ColorData(Color color)
    {
        _name = color.ToString();
        SetColor(color);
    }

    /// <summary>
    /// Refresh the color to match the values
    /// </summary>
    public void Refresh()
    {
        // Assign the color values
        _color = new Color(_red, _green, _blue, _alpha);
    }

    public void SetColor(Color color)
    {
        _color = color;

        // Assign the color values
        _red = color.r;
        _green = color.g;
        _blue = color.b;
        _alpha = color.a;
    }

    public void SetColorValues(float red, float green, float blue, float alpha)
    {
        _red = red;
        _green = green;
        _blue = blue;
        _alpha = alpha;

        _color = new Color(_red, _green, _blue, _alpha);
    }

    public void SetColorValues(Vector4 colorVector)
    {
        _red = colorVector.x;
        _green = colorVector.y;
        _blue = colorVector.z;
        _alpha = colorVector.w;

        _color = new Color(_red, _green, _blue, _alpha);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(VFX_ColorData))]
public class VFX_ColorDataCustomEditor : Editor
{
    SerializedObject _serializedObject;
    VFX_ColorData _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (VFX_ColorData)target;
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            _script.Refresh();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif