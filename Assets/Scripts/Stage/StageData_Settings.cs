using UnityEngine;

[CreateAssetMenu(menuName = "SkyWriters/StageData_Settings")]
public class StageData_Settings : ScriptableObject
{

    [SerializeField, Range(1, 60)] float _tickSpeed = 2;
    public float tickSpeed => _tickSpeed;

    [Header("Stage Size Settings")]
    [SerializeField, Range(1, 3000)] float _stageRadius = 1000;
    [SerializeField, Range(1, 3000)] float _spawnRadius = 1100;
    public float stageRadius => _stageRadius;
    public float spawnRadius => _spawnRadius;

    [Header("Environment Settings")]
    [SerializeField, Range(0, 360)] float _windDirection = 0;
    [SerializeField, Range(0, 1000)] float _windIntensity = 10;
    public float windDirection => _windDirection;
    public float windIntensity => _windIntensity;
}