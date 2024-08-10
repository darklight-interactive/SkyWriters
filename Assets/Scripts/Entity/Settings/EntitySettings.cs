using System;
using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/Entity/BaseSettings")]
public class EntitySettings : ScriptableObject
{
    [SerializeField] GameObject _prefab;
    [SerializeField] StageEntity.Data _data;
    public GameObject prefab => _prefab;
    public StageEntity.Data data => _data;
}
