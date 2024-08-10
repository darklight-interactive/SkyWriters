using System;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/StageData_Entities")]
public class StageData_Entities : ScriptableObject
{
    [Header("Cloud Settings")]
    [SerializeField] GameObject _cloudPrefab;
    [Expandable, SerializeField] StageEntityPreset _cloudPreset;

    [Header("Plane Settings")]
    [SerializeField] GameObject _planePrefab;
    [Expandable, SerializeField] StageEntityPreset _planePreset;

    [Header("Blimp Settings")]
    [SerializeField] GameObject _blimpPrefab;
    [Expandable, SerializeField] StageEntityPreset _blimpPreset;

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

}