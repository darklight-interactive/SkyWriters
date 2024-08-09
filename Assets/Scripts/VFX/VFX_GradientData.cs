using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Darklight.UnityExt.Editor;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Darklight/VFX/GradientData")]
public class VFX_GradientData : ScriptableObject
{
    [SerializeField] private Gradient _gradient;
    [SerializeField] private VFX_ColorData[] _colorDataKeys = new VFX_ColorData[4];

    public Gradient gradient => _gradient;
    public List<VFX_ColorData> colorDataKeys => _colorDataKeys.ToList();

    public VFX_GradientData(VFX_ColorData[] colorDataKeys)
    {
        _colorDataKeys = colorDataKeys;
        Refresh();
    }

    public Gradient CreateModifiedGradient(int index, VFX_ColorData colorData)
    {
        // Check if the index is valid
        if (index < 0 || index >= _colorDataKeys.Length) return _gradient;

        // Create a new array with the modified color data
        VFX_ColorData[] newKeys = _colorDataKeys;
        newKeys[index] = colorData;

        // Create a new gradient with the modified color data
        return VFX_Manager.CreateGradient(newKeys);
    }

    public void Refresh()
    {
        if (_colorDataKeys == null || _colorDataKeys.Length == 0) return;
        _gradient = VFX_Manager.CreateGradient(_colorDataKeys);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(VFX_GradientData))]
public class VFX_GradientDataCustomEditor : Editor
{
    SerializedObject _serializedObject;
    VFX_GradientData _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (VFX_GradientData)target;
        _script.Refresh();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        if (GUILayout.Button("Refresh"))
        {
            _script.Refresh();
        }

        CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.Refresh();
        }
    }
}
#endif