using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class EntitySpawnConfig
{
    [Expandable] public StageEntityPreset entityPreset;
    public List<VFX_ColorData> colors;
    [Range(0, 1)] public float spawnChance;
}

[CreateAssetMenu(menuName = "SkyWriters/SpawnerPreset")]
public class SpawnerPreset : ScriptableObject
{
    [SerializeField] Shape2D _shape2D;
    [SerializeField, Expandable] Shape2DPreset _shape2DPreset;
    [SerializeField] List<EntitySpawnConfig> _configs = new List<EntitySpawnConfig>();
    public Shape2D shape2D => _shape2DPreset.CreateShape2D(Stage.Instance.stageCenter);

    public void Initialize()
    {
        _shape2D = _shape2DPreset.CreateShape2D(Stage.Instance.stageCenter);
    }

    public StageEntityPreset GetRandomEntityByChance()
    {
        float totalChance = 0;
        foreach (var config in _configs)
        {
            totalChance += config.spawnChance;
        }

        float random = Random.Range(0, totalChance);
        float currentChance = 0;
        foreach (var config in _configs)
        {
            currentChance += config.spawnChance;
            if (random <= currentChance)
            {
                return config.entityPreset;
            }
        }

        return null;
    }
}