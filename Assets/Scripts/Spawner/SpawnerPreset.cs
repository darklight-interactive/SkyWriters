using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/SpawnerPreset")]
public class SpawnerPreset : ScriptableObject
{
    [SerializeField, Expandable] Shape2DPreset _shape2DPreset;
    [SerializeField] List<WeightedData<EntitySettings>> _entityWeightedData = new List<WeightedData<EntitySettings>>();

    public Shape2D CreateShape2D()
    {
        return _shape2DPreset.CreateShape2D(Stage.Instance.stageCenter);
    }

    public Shape2D CreateShape2D(Vector3 position)
    {
        return _shape2DPreset.CreateShape2D(position);
    }

    public EntitySettings GetRandomEntitySettings()
    {
        return WeightedDataSelector.SelectRandomWeightedItem(_entityWeightedData);
    }
}