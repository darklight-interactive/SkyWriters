using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Darklight.UnityExt.Editor;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class VFX_GradientData
{
    [SerializeField] private Gradient _gradient;
    public Gradient gradient => _gradient;


    public VFX_GradientData(Gradient gradient)
    {
        _gradient = gradient;
    }

    public VFX_GradientData(VFX_ColorDataObject colorData, float endAlpha)
    {
        _gradient = colorData.ToGradient(endAlpha);
    }

    public VFX_GradientData(VFX_ColorDataObject[] colorDataObjs)
    {
        _gradient = VFX_Manager.CreateGradient(colorDataObjs);
    }

}