using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/StageData_Entities")]
public class StageData_Entities : ScriptableObject
{
    [Header("Cloud Settings")]
    [SerializeField] GameObject _cloudPrefab;
    [Expandable, SerializeField] StageEntityPreset _cloudPreset;
    public int maxClouds = 100;


    [Header("Plane Settings")]
    [SerializeField] GameObject _planePrefab;
    [Expandable, SerializeField] StageEntityPreset _planePreset;
    public int maxPlanes = 8;


    [Header("Blimp Settings")]
    [SerializeField] GameObject _blimpPrefab;
    [Expandable, SerializeField] StageEntityPreset _blimpPreset;
    public int maxBlimps = 2;

    public T CreateEntity<T>() where T : StageEntity
    {
        GameObject prefab = null;
        StageEntityPreset preset = null;

        if (typeof(T) == typeof(CloudEntity))
        {
            prefab = _cloudPrefab;
            preset = _cloudPreset;
        }
        else if (typeof(T) == typeof(PlaneEntity))
        {
            prefab = _planePrefab;
            preset = _planePreset;
        }
        else if (typeof(T) == typeof(BlimpEntity))
        {
            prefab = _blimpPrefab;
            preset = _blimpPreset;
        }

        T newEntity = Instantiate(prefab).GetComponent<T>();
        newEntity.Initialize(preset);
        return newEntity;
    }

    public int GetMaxEntities(StageEntity.ClassType classType)
    {
        switch (classType)
        {
            case StageEntity.ClassType.CLOUD:
                return maxClouds;
            case StageEntity.ClassType.PLANE:
                return maxPlanes;
            case StageEntity.ClassType.BLIMP:
                return maxBlimps;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

}