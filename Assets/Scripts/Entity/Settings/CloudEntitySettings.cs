using System;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/Entity/CloudSettings")]
public class CloudEntitySettings : EntitySettings
{
    [Header("--- Cloud ---")]
    public VFX_ColorData defaultColor;
    public List<WeightedData<VFX_ColorData>> colorData;

    public VFX_ColorData GetRandomColorFromWeights()
    {
        return WeightedDataSelector.SelectRandomWeightedItem(colorData);
    }
}
