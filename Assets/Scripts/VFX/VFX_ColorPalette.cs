using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Darklight/VFX/ColorPalette")]
public class VFX_ColorPalette : ScriptableObject
{
    [Header("Greyscale")]
    [Expandable] public VFX_ColorData whiteColor;
    [Expandable] public VFX_ColorData blackColor;

    [Header("Primary Colors")]
    [Expandable] public VFX_ColorData redColor;
    [Expandable] public VFX_ColorData greenColor;
    [Expandable] public VFX_ColorData blueColor;

    public List<VFX_ColorData> GetColorDataList()
    {
        List<VFX_ColorData> colorDataList = new List<VFX_ColorData>
        {
            whiteColor,
            blackColor,
            redColor,
            greenColor,
            blueColor
        };

        return colorDataList;
    }

    public VFX_ColorData GetRandomColorData()
    {
        List<VFX_ColorData> colorDataList = GetColorDataList();
        return colorDataList[Random.Range(0, colorDataList.Count)];
    }

    public static Gradient CreateGradient(Color[] colors)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[colors.Length];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colors.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            colorKeys[i].color = colors[i];
            colorKeys[i].time = (float)i / (colors.Length - 1);

            alphaKeys[i].alpha = colors[i].a;
            alphaKeys[i].time = (float)i / (colors.Length - 1);
        }

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }
}