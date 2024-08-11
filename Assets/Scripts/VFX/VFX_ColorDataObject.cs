using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Darklight/VFX/ColorData")]
public class VFX_ColorDataObject : ScriptableObject
{
    // ---------------- Data ----------------
    [SerializeField] VFX_ColorData _colorData;

    // ---------------- References ----------------
    public VFX_ColorData ColorData => _colorData;
    public Color Color => _colorData.Color;

    public void Refresh() => _colorData.Refresh();

    public Gradient ToGradient(float endAlpha)
    {
        return _colorData.ToGradient(endAlpha);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(VFX_ColorDataObject))]
public class VFX_ColorDataCustomEditor : Editor
{
    SerializedObject _serializedObject;
    VFX_ColorDataObject _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (VFX_ColorDataObject)target;
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            _script.Refresh();
        }
    }
}
#endif