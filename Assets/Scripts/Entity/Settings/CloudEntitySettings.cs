using System;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/Entity/CloudSettings")]
public class CloudEntitySettings : EntitySettings
{
    [Header("--- Cloud ---")]
    public VFX_ColorDataObject defaultColor;
    public List<WeightedData<VFX_ColorDataObject>> colorData;

    public VFX_ColorDataObject GetRandomColorFromWeights()
    {
        return WeightedDataSelector.SelectRandomWeightedItem(colorData);
    }
}
