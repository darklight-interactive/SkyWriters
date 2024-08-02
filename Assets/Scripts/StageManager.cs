using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Behaviour;
using NaughtyAttributes;

using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayerInputManager))]
public class StageManager : MonoBehaviourSingleton<StageManager>
{
    public enum AreaType { ALL, STAGE, SPAWN_AREA }

    // -------------- Static Methods ------------------------
    public static Vector3 StageCenter => Instance.transform.position;
    public static float StageRadius => Instance._stageRadius;
    public static float SpawnRadiusOffset => Instance._spawnRadiusOffset;
    public static float WindDirection => Instance._windDirection;
    public static float WindIntensity => Instance._windIntensity;
    public static List<Vector3> CalculatePointsInCircle(Vector3 center, float radius, int count, Vector3 direction)
    {
        List<Vector3> points = new List<Vector3>();

        // Foreach step in the circle, calculate the points
        float angleStep = 360.0f / count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector3 newPoint = center + Quaternion.AngleAxis(angle, direction) * Vector3.right * radius;
            points.Add(newPoint);
        }
        return points;
    }

    // -------------- Properties ------------------------
    PlayerInputManager _playerInputManager => GetComponent<PlayerInputManager>();
    List<PlayerInputData> _playerInputs = new List<PlayerInputData>();

    [Header("Stage Settings")]
    [SerializeField] int _maxPlayers = 4;
    [SerializeField] float _stageRadius = 1000;
    [SerializeField, Range(10, 1000)] float _spawnRadiusOffset = 100;

    [Header("Environment Settings")]
    [SerializeField, Range(0, 360)] float _windDirection = 0;
    [SerializeField, Range(0, 1000)] float _windIntensity = 10;

    [Header("Cloud Settings")]
    [SerializeField] GameObject _cloudPrefab;
    [Expandable, SerializeField] StageEntityPreset _cloudPreset;
    [SerializeField] List<CloudData> _cloudGradients;

    [Header("Plane Settings")]
    [SerializeField] GameObject _planePrefab;
    [Expandable, SerializeField] StageEntityPreset _planePreset;

    [Header("Blimp Settings")]
    [SerializeField] GameObject _blimpPrefab;
    [Expandable, SerializeField] StageEntityPreset _blimpPreset;

    #region ================= [[ UNITY METHODS ]] ================= >>
    public override void Initialize()
    {
        if (Application.isPlaying)
        {
            _playerInputManager.onPlayerJoined += OnPlayerJoined;
            _playerInputManager.onPlayerLeft += OnPlayerLeft;
        }
    }

    void OnDrawGizmos()
    {
        // Draw the stage radius
        StageGizmos.DrawCircle(transform.position, _stageRadius, Vector3.up, Color.green);

        // Draw the spawn area
        StageGizmos.DrawCircle(transform.position, _stageRadius + _spawnRadiusOffset, Vector3.up, Color.yellow);

        // Draw the wind direction
        Gizmos.color = Color.white;
        Vector3 windDir = Quaternion.AngleAxis(_windDirection, Vector3.up) * Vector3.forward;
        Gizmos.DrawLine(transform.position, transform.position + windDir * _stageRadius);
    }
    #endregion


    #region ================= [[ STAGE MANAGEMENT ]] ================= >>

    #region (( ---- Collider Handling ---- ))
    /// <summary>
    /// Returns an array of colliders within a given radius.
    /// </summary>
    /// <param name="radius"></param>
    /// <returns></returns>
    Collider[] GetCollidersInRadius(float radius)
    {
        return Physics.OverlapSphere(transform.position, radius);
    }

    /// <summary>
    /// Returns an array of colliders within a given area type.
    /// This the main method to get colliders, each in their respective area style.      
    /// </summary>
    /// <param name="areaType"></param>
    /// <returns></returns>
    Collider[] GetCollidersInAreaType(AreaType areaType)
    {
        switch (areaType)
        {
            case AreaType.ALL:
                return FindObjectsByType<Collider>(FindObjectsSortMode.InstanceID);
            case AreaType.STAGE:
                return GetCollidersInRadius(_stageRadius);
            case AreaType.SPAWN_AREA:

                // Since the stage is inside the spawn area, we need to exclude the stage colliders from the spawn area
                List<Collider> stage = GetCollidersInRadius(_stageRadius).ToList();
                List<Collider> spawnArea = GetCollidersInRadius(_stageRadius + _spawnRadiusOffset).ToList();
                return spawnArea.Except(stage).ToArray();

            default:
                return null;
        }
    }

    /// <summary>
    /// Returns a Dictionary of colliders by their area type.   
    /// </summary>
    /// <returns></returns>
    Dictionary<AreaType, List<Collider>> GetCollidersByArea()
    {
        Dictionary<AreaType, List<Collider>> _collidersByArea = new Dictionary<AreaType, List<Collider>>();
        foreach (AreaType areaType in Enum.GetValues(typeof(AreaType)))
        {
            _collidersByArea.Add(areaType, GetCollidersInAreaType(areaType).ToList());
        }
        return _collidersByArea;
    }

    /// <summary>
    /// Checks if a collider is within a given area type.
    /// </summary>
    /// <param name="collider">
    ///     The collider to check.
    /// </param>
    /// <param name="areaType">
    ///     The area type to check.
    /// </param>
    /// <returns></returns>
    public bool IsColliderInArea(Collider collider, AreaType areaType)
    {
        return GetCollidersInAreaType(areaType).Contains(collider);
    }
    #endregion

    #region (( ---- Entity Handling ---- ))

    /// <summary>
    /// Spawns an entity of the given type at the given position.
    /// </summary>
    /// <typeparam name="T">
    ///     The data type of StageEntity to spawn.
    /// </typeparam>
    /// <param name="position">
    ///     The position to spawn the entity at.
    /// </param>
    /// <returns>
    ///     The spawned entity data.
    /// </returns>
    public T SpawnEntity<T>(Vector3 position) where T : StageEntity
    {
        StageEntity.Type type = GetEnumTypeFromSubclass<T>();
        GameObject prefab = GetEntityPrefab(type);
        T entity = Instantiate(prefab, position, Quaternion.identity).GetComponent<T>();
        entity.preset = GetStageEntityPreset(type);
        return entity;
    }

    /// <summary>
    /// Spawns an entity of the given type at a random position within the stage.
    /// </summary>
    /// <typeparam name="T">
    ///     The data type of StageEntity to spawn.
    /// </typeparam>
    /// <returns>
    ///     The spawned entity data.
    /// </returns>
    public T SpawnEntityRandomly_InStage<T>() where T : StageEntity
    {
        return SpawnEntity<T>(GetRandomPosInStage());
    }

    public List<T> SpawnEntitiesRandomly_InStage<T>(int count) where T : StageEntity
    {
        if (count <= 0)
        {
            Debug.LogError($"{Prefix} Cannot spawn 0 or less entities.", this);
            return null;
        }

        List<T> entities = new List<T>();
        for (int i = 0; i < count; i++)
        {
            T newEntity = SpawnEntityRandomly_InStage<T>();
            entities.Add(newEntity);
        }
        return entities;
    }

    public List<T> GetAllEntitiesOfType<T>() where T : StageEntity
    {
        return FindObjectsByType<T>(FindObjectsSortMode.InstanceID).ToList();
    }

    StageEntityPreset GetStageEntityPreset(StageEntity.Type entityType)
    {
        switch (entityType)
        {
            case StageEntity.Type.PLANE:
                return _planePreset;
            case StageEntity.Type.CLOUD:
                return _cloudPreset;
            case StageEntity.Type.BLIMP:
                return _blimpPreset;
            default:
                return null;
        }
    }

    GameObject GetEntityPrefab(StageEntity.Type entityType)
    {
        switch (entityType)
        {
            case StageEntity.Type.PLANE:
                return _planePrefab;
            case StageEntity.Type.CLOUD:
                return _cloudPrefab;
            case StageEntity.Type.BLIMP:
                return _blimpPrefab;
            default:
                return null;
        }
    }

    StageEntity.Type GetEnumTypeFromSubclass<T>() where T : StageEntity
    {
        Type entityType = typeof(T);
        Type stageEntityType = typeof(StageEntity);
        if (!entityType.IsSubclassOf(stageEntityType))
        {
            Debug.LogError($"Type {entityType} is not a subclass of StageEntity.");
            return StageEntity.Type.NULL;
        }

        switch (entityType)
        {
            case Type planeType when planeType == typeof(PlaneEntity):
                return StageEntity.Type.PLANE;
            case Type cloudType when cloudType == typeof(CloudEntity):
                return StageEntity.Type.CLOUD;
            case Type blimpType when blimpType == typeof(BlimpEntity):
                return StageEntity.Type.BLIMP;
            default:
                return StageEntity.Type.NULL;
        }
    }

    public CloudEntity SpawnCloudAt(Vector3 position)
    {
        CloudData gradient = _cloudGradients[Random.Range(0, _cloudGradients.Count)];

        CloudEntity newCloud = SpawnEntity<CloudEntity>(position);
        newCloud.GetComponent<CloudEntity>().SetCloudGradient(gradient);
        return newCloud;
    }
    #endregion

    #endregion

    #region ================= [[ PLAYER MANAGEMENT ]] ================= >>

    /// <summary>
    /// Called when a player joins the game.
    /// This method is called by the PlayerInputManager event : onPlayerJoined.
    /// </summary>
    /// <param name="playerInput">
    ///     The PlayerInput object of the player that joined.
    /// </param>
    void OnPlayerJoined(PlayerInput playerInput)
    {
        // Create temp data and apply base checks
        PlayerInputData playerInputData = new PlayerInputData(playerInput);

        // Check if the max players are reached        
        if (_playerInputs.Count >= _maxPlayers)
        {
            Debug.Log($"Max players reached! >> Cannot connect [ {playerInputData.GetInfo()} ]");
            return;
        }

        // Check if the player is already connected
        if (_playerInputs.Any(p => p.deviceId == playerInputData.deviceId))
        {
            Debug.Log($"{playerInputData.GetInfo()} is already connected!");
            return;
        }

        // Add the player to the list
        _playerInputs.Add(playerInputData);
        AssignPlayerToPlane(playerInputData);
    }

    /// <summary>
    /// Called when a player leaves the game.
    /// This method is called by the PlayerInputManager event : onPlayerLeft.
    /// </summary>
    /// <param name="playerInput">
    ///     The PlayerInput object of the player that left.
    /// </param>
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        // Create temp data and apply base checks
        PlayerInputData playerInputData = new PlayerInputData(playerInput);
        Debug.Log($"{playerInputData.GetInfo()} left the game!");
    }

    public PlaneEntity AssignPlayerToPlane(PlayerInputData playerInputData)
    {
        // Find the first available plane
        List<PlaneEntity> planes = GetAllEntitiesOfType<PlaneEntity>();
        foreach (PlaneEntity plane in planes)
        {
            if (plane.IsAutopilot)
            {
                plane.AssignPlayerInput(playerInputData);
                return plane;
            }
        }

        // If no planes are available, spawn a new one
        PlaneEntity newPlane = SpawnEntityRandomly_InStage<PlaneEntity>();
        newPlane.AssignPlayerInput(playerInputData);
        return newPlane;
    }

    #endregion





    // ---------------------------------------- Public Methods ---------------------------------------- >>



    /// <summary>
    /// Returns the antipodal point of a given point on the circumference of the stage.
    /// </summary>
    /// <param name="point">
    ///     The point on the circumference of the stage.
    /// </param>
    /// <returns></returns>
    public Vector3 GetAntipodalPoint(Vector3 point)
    {
        Vector3 center = transform.position; // This transform's position is the center of the stage
        Vector3 directionXZ = point - center; // Get the direction vector from the center to the point
        Vector3 antipodalPoint = center - directionXZ; // Get the antipodal point by reversing the direction vector
        return new Vector3(antipodalPoint.x, center.y, antipodalPoint.z);
    }

    /// <summary>
    /// Returns a random point within the stage.
    /// </summary>
    /// <returns>
    ///     A Vector3 representing a random point within the stage.
    ///     The y value of the vector is the same as the stage's center.
    /// </returns>
    public Vector3 GetRandomPosInStage()
    {
        Vector3 randomPoint = Random.insideUnitSphere * _stageRadius;
        randomPoint.y = transform.position.y;
        return randomPoint;
    }

    public Vector3 GetRandomPosInSpawnArea()
    {
        Vector2 point = GetRandomPointBetweenCircles(_stageRadius, _stageRadius + _spawnRadiusOffset);
        return new Vector3(point.x, transform.position.y, point.y);
    }


    Vector2 GetRandomPointBetweenCircles(float innerRadius, float outerRadius)
    {
        // Random angle in radians
        float angle = Random.Range(0, Mathf.PI * 2);

        // Random radius between innerRadius and outerRadius
        float radius = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));

        // Convert polar coordinates to Cartesian coordinates
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);

        return new Vector2(x, y);
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(StageManager))]
public class StageManagerCustomEditor : Editor
{
    SerializedObject _serializedObject;
    StageManager _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (StageManager)target;
        _script.Awake();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
