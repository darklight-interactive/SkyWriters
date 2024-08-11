using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "Darklight/VFX/ColorPalette")]
public class VFX_ColorPalette : ScriptableObject
{
    [Header("Greyscale")]
    [Expandable] public VFX_ColorDataObject transparentColor;
    [Expandable] public VFX_ColorDataObject whiteColor;
    [Expandable] public VFX_ColorDataObject blackColor;

    [Header("Primary Colors")]
    [Expandable] public VFX_ColorDataObject redColor;
    [Expandable] public VFX_ColorDataObject greenColor;
    [Expandable] public VFX_ColorDataObject blueColor;

    [Header("Secondary Colors")]
    [Expandable] public VFX_ColorDataObject yellowColor;
    [Expandable] public VFX_ColorDataObject cyanColor;
    [Expandable] public VFX_ColorDataObject magentaColor;

    public List<VFX_ColorDataObject> GetColorDataList()
    {
        List<VFX_ColorDataObject> colorDataList = new List<VFX_ColorDataObject>
        {
            whiteColor,
            blackColor,
            redColor,
            greenColor,
            blueColor
        };

        return colorDataList;
    }

    public VFX_ColorDataObject GetRandomColorData()
    {
        List<VFX_ColorDataObject> colorDataList = GetColorDataList();
        return colorDataList[Random.Range(0, colorDataList.Count)];
    }
}