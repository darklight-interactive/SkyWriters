using System;
using Darklight.UnityExt.Editor;
using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/StageEntityPreset")]
public class StageEntityPreset : ScriptableObject
{
    [SerializeField] StageEntity.Class _type;

    [Header("--- Collider ---")]
    [SerializeField, Range(1, 500)] float _colliderHeight;
    [SerializeField, Range(1, 500)] float _colliderRadius;

    [Header("--- Speed ---")]
    [SerializeField, Range(0, 1000)] float _moveSpeed;
    [SerializeField, Range(0, 10)] float _rotationSpeed;

    [Header("--- Stats ---")]
    [SerializeField, Range(0, 1)] float _windResistance;

    [Header("--- Gameplay ---")]
    [SerializeField] bool _respawnOnExit;
    [SerializeField, Range(-1, 999)] float _lifeSpan;

    // ----------------- Getters -----------------
    public StageEntity.Class type => _type;
    public float colliderHeight => _colliderHeight;
    public float colliderRadius => _colliderRadius;
    public float moveSpeed => _moveSpeed;
    public float rotationSpeed => _rotationSpeed;
    public float windResistance => _windResistance;
    public bool respawnOnExit => _respawnOnExit;
    public float lifeSpan => _lifeSpan;

}
