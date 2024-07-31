using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using JetBrains.Annotations;
using NaughtyAttributes;

using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerInputManager))]
public class StageManager : MonoBehaviourSingleton<StageManager>
{
    public enum AreaType { ALL, STAGE, SPAWN_AREA }

    // -------------- Static Methods ------------------------
    public static float StageHeight => Instance.transform.position.y;

    /// <summary>
    /// Assigns an entity to the stage.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="col_height"></param>
    public static void AssignEntityToStage(StageEntity entity, float col_height = 1)
    {
        float stageHeight = StageHeight + (col_height / 2);
        entity.currentPosition = new Vector3(entity.currentPosition.x, stageHeight, entity.currentPosition.z);
    }


    // -------------- Properties ------------------------

    // << Player Input Manager >>
    PlayerInputManager _playerInputManager => GetComponent<PlayerInputManager>();
    int _maxPlayers = 4;
    List<StagePlayerData> _playerInputs = new List<StagePlayerData>();

    [Header("Stage Data")]
    [SerializeField] private float _stageRadius = 1000;
    [SerializeField, Range(10, 1000)] private float _spawnRadiusOffset = 100;

    [Header("Cloud Data")]
    [SerializeField] List<CloudGradientData> _cloudGradients;
    [SerializeField] float _cloudSpeed = 10f;
    public float CloudSpeed => _cloudSpeed;

    [Header("Prefabs")]
    [SerializeField] GameObject _planePrefab;
    [SerializeField] GameObject _cloudPrefab;



    #region ================= [[ UNITY METHODS ]] ================= >>
    public override void Initialize()
    {
        _playerInputManager.onPlayerJoined += OnPlayerJoined;
        _playerInputManager.onPlayerLeft += OnPlayerLeft;

        SpawnEntitiesRandomly_InStage<CloudEntity>(10);
    }

    public void Update()
    {
        //_stageColliders = Physics.OverlapSphere(transform.position, _stageRadius).ToList();
        //_spawnAreaColliders = Physics.OverlapSphere(transform.position, _stageRadius + _spawnRadiusOffset).ToList();
    }

    void OnDrawGizmos()
    {
        // Draw the stage radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _stageRadius);

        // Draw the spawn offset
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _stageRadius + _spawnRadiusOffset);
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
                return GetCollidersInRadius(_stageRadius + _spawnRadiusOffset);
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
        return Instantiate(prefab, position, Quaternion.identity).GetComponent<T>();
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
    GameObject GetEntityPrefab(StageEntity.Type entityType)
    {
        switch (entityType)
        {
            case StageEntity.Type.PLANE:
                return _planePrefab;
            case StageEntity.Type.CLOUD:
                return _cloudPrefab;
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
        CloudGradientData gradient = _cloudGradients[Random.Range(0, _cloudGradients.Count)];

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
        StagePlayerData playerInputData = new StagePlayerData(playerInput);

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
        StagePlayerData playerInputData = new StagePlayerData(playerInput);
        Debug.Log($"{playerInputData.GetInfo()} left the game!");
    }

    public PlaneEntity AssignPlayerToPlane(StagePlayerData playerInputData)
    {
        PlaneEntity newPlane = null;

        // Find the first available plane
        List<PlaneEntity> planes = GetAllEntitiesOfType<PlaneEntity>();
        foreach (PlaneEntity plane in planes)
        {
            if (plane.IsAutopilot)
            {
                newPlane = plane;
                newPlane.AssignPlayerInput(playerInputData);
                break;
            }
        }

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
