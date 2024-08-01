using System;
using Darklight.UnityExt.Editor;
using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/StageEntityPreset")]
public class StageEntityPreset : ScriptableObject
{
    [SerializeField] StageEntity.Type _type;

    [Header("--- Collider ---")]
    [SerializeField, Range(1, 500)] float _colliderHeight;
    [SerializeField, Range(1, 500)] float _colliderRadius;

    [Header("--- Speed ---")]
    [SerializeField, Range(0, 1000)] float _moveSpeed;
    [SerializeField, Range(0, 10)] float _rotationSpeed;

    [Header("--- Gameplay ---")]
    [SerializeField] bool _respawnOnExit;
    [SerializeField, Range(-1, 999)] float _lifeSpan;

    public StageEntity.Data ToData()
    {
        StageEntity.Data data = new StageEntity.Data(_type, _respawnOnExit, _colliderHeight, _colliderRadius, _moveSpeed, _rotationSpeed, _lifeSpan);

        return data;
    }
}
